using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents a mileage bucket with associated service recommendations
/// </summary>
public class ServiceInterval
{
    /// <summary>
    /// Mileage bucket value (required if serviceIntervals is not null)
    /// </summary>
    [JsonPropertyName("mileage")]
    public required int Mileage { get; set; }

    /// <summary>
    /// List of recommended services for this mileage bucket (required if serviceIntervals is not null)
    /// </summary>
    [JsonPropertyName("services")]
    public required ICollection<RecommendedService> Services { get; set; } = new List<RecommendedService>();
}
