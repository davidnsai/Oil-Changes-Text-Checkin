namespace TextCheckIn.Data.OmniX.Models;

/// <summary>
/// Base class representing common vehicle and location information from OmniX.
/// </summary>
public abstract class OmniXVehicleRequest
{
    /// <summary>
    /// OmniX location identifier
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// Client's location identifier
    /// </summary>
    public required string ClientLocationId { get; set; }

    /// <summary>
    /// Request timestamp
    /// </summary>
    public required DateTime Datetime { get; set; }

    /// <summary>
    /// Vehicle license plate number
    /// </summary>
    public required string LicensePlate { get; set; }

    /// <summary>
    /// State code for the license plate
    /// </summary>
    public required string StateCode { get; set; }

    /// <summary>
    /// Vehicle make
    /// </summary>
    public string? Make { get; set; }

    /// <summary>
    /// Vehicle model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Vehicle year
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Vehicle identification number
    /// </summary>
    public string? Vin { get; set; }

    /// <summary>
    /// Estimated mileage of the vehicle
    /// </summary>
    public int? EstimatedMileage { get; set; }
}
