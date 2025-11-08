namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents vehicle information collected from omniX or user input
/// </summary>
public class VehicleData
{
    /// <summary>
    /// License plate number
    /// </summary>
    public required string LicensePlate { get; set; }

    /// <summary>
    /// State code for license plate
    /// </summary>
    public required string StateCode { get; set; }

    /// <summary>
    /// Vehicle make (e.g., "HONDA")
    /// </summary>
    public string? Make { get; set; }

    /// <summary>
    /// Vehicle model (e.g., "CIVIC")
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Vehicle year
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Vehicle color
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Vehicle VIN number
    /// </summary>
    public string? Vin { get; set; }

    /// <summary>
    /// Current odometer reading (actual mileage from customer)
    /// </summary>
    public int? ActualMileage { get; set; }

    /// <summary>
    /// Estimated mileage from omniX
    /// </summary>
    public int? EstimatedMileage { get; set; }

    /// <summary>
    /// Last service date from history
    /// </summary>
    public DateOnly? LastServiceDate { get; set; }

    /// <summary>
    /// Last service mileage from history
    /// </summary>
    public int? LastServiceMileage { get; set; }
}

/// <summary>
/// Source of vehicle data
/// </summary>
public enum VehicleDataSource
{
    /// <summary>
    /// Data received from omniX webhook
    /// </summary>
    OmniXWebhook,

    /// <summary>
    /// Data retrieved from omniX API (fallback)
    /// </summary>
    OmniXApi,

    /// <summary>
    /// Data entered manually by customer
    /// </summary>
    Manual,

    /// <summary>
    /// Data from mock service (development)
    /// </summary>
    Mock
}
