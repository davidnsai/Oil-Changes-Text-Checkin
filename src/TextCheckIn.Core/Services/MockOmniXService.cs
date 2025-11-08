using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Shared.Models;

namespace TextCheckIn.Core.Services;

/// <summary>
/// Mock implementation of omniX service for development and testing
/// </summary>
public class MockOmniXService : OmniXServiceBase
{
    private readonly Random _random = new();
    private readonly ILogger<MockOmniXService> _logger;
    private readonly ILogger<OmniXServiceBase> _baseLogger;
    private readonly OmniXConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockOmniXService"/> class.
    /// </summary>
    /// <param name="baseLogger">The base logger for the OmniXServiceBase class.</param>
    /// <param name="logger">The logger for this service.</param>
    /// <param name="omnixConfig">The configuration options for the OmniX service.</param>
    public MockOmniXService(
        ILogger<OmniXServiceBase> baseLogger,
        ILogger<MockOmniXService> logger,
        IOptions<OmniXConfiguration> omnixConfig): base(baseLogger)
    {
        _logger = logger;
        _config = omnixConfig.Value;
        _baseLogger = baseLogger;
    }

    /// <summary>
    /// Gets service recommendations for a vehicle based on its VIN.
    /// </summary>
    /// <param name="request">The request containing VIN and other vehicle information.</param>
    /// <returns>A task that represents the asynchronous operation, containing the service recommendations or null if not found.</returns>
    public override async Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByVinRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets service recommendations for a vehicle based on its license plate.
    /// </summary>
    /// <param name="request">The request containing license plate and other vehicle information.</param>
    /// <returns>A task that represents the asynchronous operation, containing the service recommendations or null if not found.</returns>
    public override async Task<List<CheckInService>> GetServiceRecommendationAsync(
        GetServiceRecommendationsByLicensePlateRequest request)
    {
        //_logger.LogInformation("Mock API: Getting recommendations for {LicensePlate} in {StateCode}",
        //   request.LicensePlate, request.StateCode);

        //// Simulate API delay
        //if (_config.Mock.DelayMs > 0)
        //{
        //    await Task.Delay(_config.Mock.DelayMs);
        //}

        //// Simulate API errors
        //if (_random.NextDouble() < _config.Mock.ErrorProbability)
        //{
        //    _logger.LogWarning("Mock API: Simulating API error for {LicensePlate}", request.LicensePlate);
        //    throw new HttpRequestException("Mock API error: Service temporarily unavailable");
        //}

        //// Simulate vehicle not found
        //if (_random.NextDouble() < _config.Mock.NotFoundProbability ||
        //    !MockVehicles.ContainsKey(request.LicensePlate.ToUpperInvariant()))
        //{
        //    _logger.LogInformation("Mock API: Vehicle not found for {LicensePlate}", request.LicensePlate);
        //    return null;
        //}

        //var mockVehicle = MockVehicles[request.LicensePlate.ToUpperInvariant()];
        //var incomingServiceRecommendation = GenerateIncomingServiceRecommendation(mockVehicle);

        //return ProcessServiceRecommendation(incomingServiceRecommendation, request.Mileage);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Processes incoming service recommendations.
    /// </summary>
    /// <param name="notification">The incoming service recommendation to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented in the mock service.</exception>
    public override async Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sample vehicle data used for mocking responses.
    /// </summary>
    private static readonly Dictionary<string, MockVehicleData> MockVehicles = new()
    {
        ["ABC123"] = new MockVehicleData("ABC123", "CA", "HONDA", "CIVIC", 2018, "1HGBH41JXMN109186", 92500),
        ["DEF456"] = new MockVehicleData("DEF456", "CA", "TOYOTA", "CAMRY", 2020, "4T1BE46K89U123456", 45000),
        ["GHI789"] = new MockVehicleData("GHI789", "TX", "FORD", "F150", 2019, "1FTEW1E59KFA12345", 78000),
        ["JKL012"] = new MockVehicleData("JKL012", "NY", "CHEVROLET", "MALIBU", 2017, "1G1ZE5ST5HF123456", 105000),
        ["MNO345"] = new MockVehicleData("MNO345", "FL", "NISSAN", "ALTIMA", 2021, "1N4AL3AP6MC123456", 32000),
    };

    /// <summary>
    /// Generates a mock service recommendation for a vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle data to generate recommendations for.</param>
    /// <param name="storeId">The client location identifier.</param>
    /// <returns>A mock service recommendation.</returns>
    private ServiceRecommendation GenerateIncomingServiceRecommendation(MockVehicleData vehicle, string storeId)
    {
        var shouldHavePartialData = _random.NextDouble() < _config.Mock.PartialDataProbability;

        return new ServiceRecommendation
        {
            LocationId = Guid.NewGuid(),
            ClientLocationId = storeId,
            Datetime = DateTime.UtcNow,
            LicensePlate = vehicle.LicensePlate,
            StateCode = vehicle.StateCode,
            Make = shouldHavePartialData && _random.NextDouble() < 0.3 ? null : vehicle.Make,
            Model = shouldHavePartialData && _random.NextDouble() < 0.3 ? null : vehicle.Model,
            Year = shouldHavePartialData && _random.NextDouble() < 0.2 ? null : vehicle.Year,
            Vin = shouldHavePartialData && _random.NextDouble() < 0.4 ? null : vehicle.Vin,
            LastServiceDate = GenerateLastServiceDate(),
            LastServiceMileage = GenerateLastServiceMileage(vehicle.EstimatedMileage),
            EstimatedMileage = vehicle.EstimatedMileage,
            ServiceIntervals = GenerateMockServiceIntervals(vehicle.EstimatedMileage).ToList()
        };
    }

    /// <summary>
    /// Generates a random last service date.
    /// </summary>
    /// <returns>A random date representing the last service date, or null.</returns>
    private DateOnly? GenerateLastServiceDate()
    {
        // 70% chance of having service history
        if (_random.NextDouble() < 0.7)
        {
            var daysAgo = _random.Next(30, 365);
            return DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
        }
        return null;
    }

    /// <summary>
    /// Generates a random last service mileage based on the current estimated mileage.
    /// </summary>
    /// <param name="estimatedMileage">The current estimated mileage of the vehicle.</param>
    /// <returns>A random mileage representing the last service mileage, or null.</returns>
    private int? GenerateLastServiceMileage(int estimatedMileage)
    {
        // If there's a service date, generate corresponding mileage
        if (_random.NextDouble() < 0.7)
        {
            var mileageReduction = _random.Next(5000, 25000);
            return Math.Max(0, estimatedMileage - mileageReduction);
        }
        return null;
    }

    /// <summary>
    /// Generates mock service intervals based on the estimated mileage.
    /// </summary>
    /// <param name="estimatedMileage">The estimated mileage of the vehicle.</param>
    /// <returns>A collection of service intervals.</returns>
    private ICollection<ServiceInterval> GenerateMockServiceIntervals(int estimatedMileage)
    {
        var intervals = new List<ServiceInterval>();

        // Generate mileage buckets around estimated mileage
        var buckets = new[]
        {
            estimatedMileage - 7500,
            estimatedMileage - 5000,
            estimatedMileage - 2500,
            estimatedMileage,
            estimatedMileage + 2500,
            estimatedMileage + 5000,
            estimatedMileage + 7500
        }.Where(m => m > 0).ToArray();

        foreach (var mileage in buckets)
        {
            var services = GenerateServicesForMileage(mileage);
            if (services.Any())
            {
                intervals.Add(new ServiceInterval
                {
                    Mileage = mileage,
                    Services = [.. services]
                });
            }
        }

        return intervals;
    }

    /// <summary>
    /// Generates recommended services for a specific mileage point.
    /// </summary>
    /// <param name="mileage">The mileage point to generate services for.</param>
    /// <returns>A collection of recommended services.</returns>
    private IEnumerable<RecommendedService> GenerateServicesForMileage(int mileage)
    {
        var services = new List<RecommendedService>();

        // Generate realistic service recommendation based on mileage
        foreach (var power6Service in Power6ServiceCatalog.Services)
        {
            if (ShouldRecommendService(power6Service, mileage))
            {
                services.Add(new RecommendedService
                {
                    Id = power6Service.Id,
                    Name = power6Service.Name,
                    IntervalMiles = GenerateServiceInterval(power6Service),
                    LastServiceMiles = GenerateLastServiceMileage(mileage)
                });
            }
        }

        return services;
    }

    /// <summary>
    /// Determines whether a specific service should be recommended based on the mileage.
    /// </summary>
    /// <param name="service">The service to evaluate.</param>
    /// <param name="mileage">The current mileage of the vehicle.</param>
    /// <returns>True if the service should be recommended, otherwise false.</returns>
    private bool ShouldRecommendService(Power6Service service, int mileage)
    {
        // Different services have different recommendation patterns
        return service.Id switch
        {
            Power6ServiceCatalog.TransmissionServiceId => mileage >= 60000 && mileage % 30000 < 5000,
            Power6ServiceCatalog.CoolantServiceId => mileage >= 30000 && mileage % 50000 < 7500,
            Power6ServiceCatalog.IntakeCleaningServiceId => mileage >= 40000 && mileage % 40000 < 5000,
            Power6ServiceCatalog.PowerSteeringServiceId => mileage >= 50000 && mileage % 25000 < 5000,
            Power6ServiceCatalog.GearBoxServiceId => mileage >= 75000 && mileage % 35000 < 5000,
            Power6ServiceCatalog.CabinAirFilterServiceId => mileage % 15000 < 2500,
            _ => _random.NextDouble() < 0.3
        };
    }

    /// <summary>
    /// Generates a service interval for a specific service.
    /// </summary>
    /// <param name="service">The service to generate an interval for.</param>
    /// <returns>The interval in miles.</returns>
    private int GenerateServiceInterval(Power6Service service)
    {
        return service.Id switch
        {
            Power6ServiceCatalog.TransmissionServiceId => 30000 + _random.Next(-2000, 2000),
            Power6ServiceCatalog.CoolantServiceId => 50000 + _random.Next(-5000, 5000),
            Power6ServiceCatalog.IntakeCleaningServiceId => 40000 + _random.Next(-5000, 5000),
            Power6ServiceCatalog.PowerSteeringServiceId => 25000 + _random.Next(-2000, 2000),
            Power6ServiceCatalog.GearBoxServiceId => 35000 + _random.Next(-3000, 3000),
            Power6ServiceCatalog.CabinAirFilterServiceId => 15000 + _random.Next(-1000, 1000),
            _ => 30000
        };
    }

    /// <summary>
    /// Represents mock vehicle data for testing.
    /// </summary>
    /// <param name="LicensePlate">The license plate of the vehicle.</param>
    /// <param name="StateCode">The state code where the vehicle is registered.</param>
    /// <param name="Make">The make of the vehicle.</param>
    /// <param name="Model">The model of the vehicle.</param>
    /// <param name="Year">The year the vehicle was manufactured.</param>
    /// <param name="Vin">The vehicle identification number.</param>
    /// <param name="EstimatedMileage">The estimated mileage of the vehicle.</param>
    private record MockVehicleData(
        string LicensePlate,
        string StateCode,
        string Make,
        string Model,
        int Year,
        string Vin,
        int EstimatedMileage);
}
