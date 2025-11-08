using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents a recommended service within a mileage interval
/// </summary>
public class RecommendedService
{
    /// <summary>
    /// Service UUID identifier (constant, never changes)
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Service display name (may change over time)
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Recommended mileage interval for this service
    /// </summary>
    [JsonPropertyName("intervalMiles")]
    public required int IntervalMiles { get; set; }

    /// <summary>
    /// Last service mileage from history (optional)
    /// </summary>
    [JsonPropertyName("lastServiceMiles")]
    public int? LastServiceMiles { get; set; }
}
