using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.OmniX.Models;

namespace TextCheckIn.Core.Services
{
    public class OmniXService : OmniXServiceBase
    {
        private readonly OmniXConfiguration _config;
        private readonly ILogger<OmniXService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICheckInRepository _checkInRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICheckInServiceRepository _checkInServiceRepository;
        private readonly AppDbContext _dbContext;

        public OmniXService(
            IOptions<OmniXConfiguration> options,
            ILogger<OmniXService> logger,
            IVehicleRepository vehicleRepository,
            ICheckInRepository checkInRepository,
            IServiceRepository serviceRepository,
            ICheckInServiceRepository checkInServiceRepository,
            AppDbContext dbContext) : base(logger)
        {
            _config = options.Value;
            _logger = logger;
            _vehicleRepository = vehicleRepository;
            _checkInRepository = checkInRepository;
            _serviceRepository = serviceRepository;
            _checkInServiceRepository = checkInServiceRepository;
            _dbContext = dbContext;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public override async Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByVinRequest request)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByLicensePlateRequest request)
        {
            var checkInServices = await _checkInServiceRepository.GetCheckInServicesByCheckInUuidAsync(request.CheckInId);

            return checkInServices;            
        }

        public override async Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Processing incoming service recommendation for {LicensePlate} ({StateCode})",
                    notification.LicensePlate, notification.StateCode);

                // make sure the recommendation id is unique
                var existingCheckIn = await _checkInRepository.GetCheckInByUuidAsync(notification.Id);
                if (existingCheckIn != null)
                {
                    throw new InvalidOperationException($"A check-in with Recommendation ID {notification.Id} already exists.");
                }

                // Step 1: Check if vehicle exists by license plate and state
                var vehicle = _vehicleRepository.GetVehicleByLicensePlateAndState(notification.LicensePlate, notification.StateCode);

                if (vehicle == null)
                {
                    // Create new vehicle
                    vehicle = new Vehicle
                    {
                        VehicleUUID = Guid.NewGuid(),
                        LicensePlate = notification.LicensePlate,
                        StateCode = notification.StateCode,
                        Make = notification.Make,
                        Model = notification.Model,
                        YearOfMake = notification.Year,
                        Vin = notification.Vin,
                        LastServiceDate = notification.LastServiceDate?.ToDateTime(TimeOnly.MinValue),
                        LastServiceMileage = notification.LastServiceMileage,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // make sure the VIN is unique
                    if (!string.IsNullOrEmpty(vehicle.Vin))
                    {
                        var existingVinVehicle = _vehicleRepository.GetVehicleByVin(vehicle.Vin);
                        if (existingVinVehicle != null)
                        {
                            throw new InvalidOperationException($"A vehicle with VIN {vehicle.Vin} already exists.");
                        }
                    }

                    var vehicleAdded = _vehicleRepository.AddVehicle(vehicle);
                    if (!vehicleAdded)
                    {
                        throw new InvalidOperationException($"Failed to create vehicle for license plate {notification.LicensePlate}");
                    }

                    _logger.LogInformation("Created new vehicle with license plate {LicensePlate}", notification.LicensePlate);
                }
                else
                {
                    // Update vehicle information if provided
                    if (!string.IsNullOrEmpty(notification.Make))
                        vehicle.Make = notification.Make;
                    if (!string.IsNullOrEmpty(notification.Model))
                        vehicle.Model = notification.Model;
                    if (notification.Year.HasValue)
                        vehicle.YearOfMake = notification.Year;
                    if (!string.IsNullOrEmpty(notification.Vin))
                        vehicle.Vin = notification.Vin;
                    if (notification.LastServiceDate.HasValue)
                        vehicle.LastServiceDate = notification.LastServiceDate.Value.ToDateTime(TimeOnly.MinValue);
                    if (notification.LastServiceMileage.HasValue)
                        vehicle.LastServiceMileage = notification.LastServiceMileage;

                    vehicle.UpdatedAt = DateTime.UtcNow;
                    _vehicleRepository.UpdateVehicle(vehicle);
                }

                // Step 2: Validate all services exist in the database
                var allServices = _serviceRepository.GetAllServices();
                var serviceIds = notification.ServiceIntervals.SelectMany(si => si.Services.Select(s => s.Id)).ToList();

                // check if all services exist in the database
                foreach (var serviceId in serviceIds)
                {
                    var service = _serviceRepository.GetServiceByUuid(Guid.Parse(serviceId)) ?? throw new InvalidOperationException($"Service {serviceId} not found in database");
                }

                // Step 3: Create check-in
                var checkIn = new CheckIn
                {
                    VehicleId = vehicle.Id,
                    Uuid = notification.Id,
                    ClientLocationId = int.Parse(notification.ClientLocationId),
                    OmnixLocationId = notification.LocationId,
                    EstimatedMileage = notification.EstimatedMileage,
                    IsProcessed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var checkInAdded = _checkInRepository.AddCheckIn(checkIn);
                if (!checkInAdded)
                {
                    throw new InvalidOperationException("Failed to create check-in");
                }

                _logger.LogInformation("Created check-in {CheckInId} for vehicle {VehicleId}", checkIn.Id, vehicle.Id);

                foreach (var serviceInterval in notification.ServiceIntervals)
                {
                    foreach (var service in serviceInterval.Services)
                    {
                        var serviceEntity = allServices.FirstOrDefault(s => s.ServiceUuid == Guid.Parse(service.Id));
                        if (serviceEntity == null)
                        {
                            _logger.LogWarning("Service {ServiceId} not found in database, skipping", service.Id);
                            continue;
                        }

                        var checkInService = new CheckInService
                        {
                            CheckInId = checkIn.Id,
                            ServiceId = serviceEntity.Id,
                            IsCustomerSelected = false,
                            IntervalMiles = service.IntervalMiles,
                            LastServiceMiles = service.LastServiceMiles,
                            MileageBucket = serviceInterval.Mileage
                        };

                        var checkInServiceAdded = _checkInServiceRepository.AddCheckInService(checkInService);
                        if (!checkInServiceAdded)
                        {
                            _logger.LogWarning("Failed to add service {ServiceId} to check-in {CheckInId}", service.Id, checkIn.Id);
                        }
                    }
                }

                // Commit transaction
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error processing incoming recommendation for {LicensePlate}: {Message}",
                    notification.LicensePlate, ex.Message);
                throw; // Re-throw to allow proper error handling by the caller
            }
        }
    }
}
