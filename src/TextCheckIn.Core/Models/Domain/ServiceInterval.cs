using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

public class ServiceInterval
{
    [JsonPropertyName("mileage")]
    public required int Mileage { get; set; }

    [JsonPropertyName("services")]
    public required ICollection<RecommendedService> Services { get; set; } = new List<RecommendedService>();
}
