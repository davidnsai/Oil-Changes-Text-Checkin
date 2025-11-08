using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents a notification payload received from omniX Sales Navigator webhook
/// </summary>
public class OmniXNotification
{
    /// <summary>
    /// omniX location identifier (required)
    /// </summary>
    [JsonPropertyName("locationId")]
    public required string LocationId { get; set; }

    /// <summary>
    /// Oil Changers store identifier (required)
    /// </summary>
    [JsonPropertyName("clientLocationId")]
    public required string ClientLocationId { get; set; }

    /// <summary>
    /// Notification timestamp in UTC (required)
    /// </summary>
    [JsonPropertyName("datetime")]
    public required DateTime DateTime { get; set; }

    /// <summary>
    /// Detected license plate number (required)
    /// </summary>
    [JsonPropertyName("licensePlate")]
    public required string LicensePlate { get; set; }

    /// <summary>
    /// Two-symbol state code (required)
    /// </summary>
    [JsonPropertyName("stateCode")]
    public required string StateCode { get; set; }

    /// <summary>
    /// Vehicle make (optional - can be null if detection failed)
    /// </summary>
    [JsonPropertyName("make")]
    public string? Make { get; set; }

    /// <summary>
    /// Vehicle model (optional - can be null if detection failed)
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Vehicle year (optional - can be null if detection failed)
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    /// <summary>
    /// Vehicle VIN (optional - can be null if not available)
    /// </summary>
    [JsonPropertyName("vin")]
    public string? Vin { get; set; }

    /// <summary>
    /// Last registered service date (optional)
    /// </summary>
    [JsonPropertyName("lastServiceDate")]
    public DateOnly? LastServiceDate { get; set; }

    /// <summary>
    /// Last registered service mileage (optional)
    /// </summary>
    [JsonPropertyName("lastServiceMileage")]
    public int? LastServiceMileage { get; set; }

    /// <summary>
    /// Estimated current mileage (optional)
    /// </summary>
    [JsonPropertyName("estimatedMileage")]
    public int? EstimatedMileage { get; set; }

    /// <summary>
    /// Service recommendations organized by mileage intervals (optional)
    /// </summary>
    [JsonPropertyName("serviceIntervals")]
    public ICollection<ServiceInterval>? ServiceIntervals { get; set; }
}
