using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Data.Entities;
using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.OmniX.Models;
using System.Net.Http;

namespace TextCheckIn.Core.Services
{
    public class OmniXService : IOmniXService
    {
        private readonly OmniXConfiguration _config;
        private readonly ILogger<OmniXService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICheckInRepository _checkInRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICheckInServiceRepository _checkInServiceRepository;
        private readonly AppDbContext _dbContext;
        private readonly MileageBucketService _mileageBucketService;
        private readonly HttpClient _httpClient;

        public OmniXService(
            IOptions<OmniXConfiguration> options,
            ILogger<OmniXService> logger,
            IVehicleRepository vehicleRepository,
            ICheckInRepository checkInRepository,
            IServiceRepository serviceRepository,
            ICheckInServiceRepository checkInServiceRepository,
            AppDbContext dbContext,
            MileageBucketService mileageBucketService,
            IHttpClientFactory httpClientFactory)
        {
            _config = options.Value;
            _logger = logger;
            _vehicleRepository = vehicleRepository;
            _checkInRepository = checkInRepository;
            _serviceRepository = serviceRepository;
            _checkInServiceRepository = checkInServiceRepository;
            _dbContext = dbContext;
            _mileageBucketService = mileageBucketService;
            _httpClient = httpClientFactory.CreateClient("OmniXApi");

            // Use the globally configured JSON options, but create a new instance to avoid modifying the shared options
            _jsonOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true // Keep this for deserialization
            };
        }

        public async Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByCheckInUuidRequest request)
        {
            // Get the check-in to access EstimatedMileage and ActualMileage
            var checkIn = await _checkInRepository.GetCheckInByUuidAsync(request.CheckInUuid);
            if (checkIn == null)
            {
                _logger.LogWarning("Check-in with UUID {CheckInUuid} not found", request.CheckInUuid);
                return new List<CheckInService>();
            }

            // Get all CheckInService records for the check-in
            var allCheckInServices = await _checkInServiceRepository.GetCheckInServicesByCheckInUuidAsync(request.CheckInUuid);

            if (!allCheckInServices.Any())
            {
                return new List<CheckInService>();
            }

            // Extract unique MileageBucket values from the services
            var availableBuckets = allCheckInServices.Select(cs => cs.MileageBucket).Distinct().ToList();

            // Determine actual mileage: use request.Mileage if provided, otherwise use CheckIn.ActualMileage
            var actualMileage = request.Mileage ?? checkIn.ActualMileage;

            // Use MileageBucketService to select the appropriate bucket
            var selectedBucket = _mileageBucketService.SelectMileageBucket(
                actualMileage,
                checkIn.EstimatedMileage,
                availableBuckets);

            _logger.LogInformation(
                "Selecting mileage bucket {SelectedBucket} for check-in {CheckInUuid}. Actual: {ActualMileage}, Estimated: {EstimatedMileage}",
                selectedBucket, request.CheckInUuid, actualMileage, checkIn.EstimatedMileage);

            // Filter services to only those matching the selected bucket
            var filteredServices = allCheckInServices
                .Where(cs => cs.MileageBucket == selectedBucket)
                .ToList();

            return filteredServices;
        }

        public async Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification)
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

        public async Task SubmitWorkOrderAsync(Guid checkInUuid, CheckIn checkIn)
        {
            try
            {
                _logger.LogInformation("Submitting work order for check-in {CheckInUuid}", checkInUuid);

                if (checkIn.Vehicle == null)
                {
                    throw new InvalidOperationException($"Vehicle not found for check-in {checkInUuid}");
                }

                if (checkIn.OmnixLocationId == null)
                {
                    throw new InvalidOperationException($"Location ID is required for check-in {checkInUuid}");
                }

                if (checkIn.ClientLocationId == null)
                {
                    throw new InvalidOperationException($"Client location ID is required for check-in {checkInUuid}");
                }

                var recommendedServices = checkIn.CheckInServices.Select(cs => new RecommendedService
                {
                    Id = cs.Service.ServiceUuid.ToString(),
                    Name = cs.Service.Name,
                    IntervalMiles = cs.IntervalMiles,
                    LastServiceMiles = cs.LastServiceMiles,
                    SelectedByClient = cs.IsCustomerSelected
                }).ToList();

                var workOrderRequest = new WorkOrder
                {
                    RecommendationId = checkIn.Uuid,
                    Datetime = checkIn.DateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ClientLocationId = checkIn.ClientLocationId.Value.ToString(),
                    LocationId = checkIn.OmnixLocationId.Value,
                    LicensePlate = checkIn.Vehicle.LicensePlate,
                    StateCode = checkIn.Vehicle.StateCode ?? string.Empty,
                    Make = checkIn.Vehicle.Make,
                    Model = checkIn.Vehicle.Model,
                    Year = checkIn.Vehicle.YearOfMake,
                    Vin = checkIn.Vehicle.Vin,
                    EstimatedMileage = checkIn.EstimatedMileage,
                    ActualMileage = checkIn.ActualMileage,
                    Services = recommendedServices
                };

                // Serialize with your custom options
                var json = JsonSerializer.Serialize(workOrderRequest, _jsonOptions);
                // Create content with proper encoding
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Make the POST request
                var response = await _httpClient.PostAsync("salesnavigator/workorders", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Successfully submitted work order for check-in {CheckInUuid}. Status: {StatusCode}",
                        checkInUuid, response.StatusCode);
                }
                else
                {
                    _logger.LogError(
                        "Failed to submit work order for check-in {CheckInUuid}. Status: {StatusCode}, Response: {ErrorContent}",
                        checkInUuid, response.StatusCode, responseBody);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error submitting work order for check-in {CheckInUuid}", checkInUuid);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting work order for check-in {CheckInUuid}: {Message}", checkInUuid, ex.Message);
                throw;
            }
        }
    }
}
