using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

public class RecommendedService
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("intervalMiles")]
    public required int IntervalMiles { get; set; }

    [JsonPropertyName("lastServiceMiles")]
    public int? LastServiceMiles { get; set; }
}
