using System.Text.Json.Serialization;

namespace TextCheckIn.Core.Models.Domain;

public class OmniXNotification
{
    [JsonPropertyName("locationId")]
    public required string LocationId { get; set; }

    [JsonPropertyName("clientLocationId")]
    public required string ClientLocationId { get; set; }

    [JsonPropertyName("datetime")]
    public required DateTime DateTime { get; set; }

    [JsonPropertyName("licensePlate")]
    public required string LicensePlate { get; set; }

    [JsonPropertyName("stateCode")]
    public required string StateCode { get; set; }

    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("vin")]
    public string? Vin { get; set; }

    [JsonPropertyName("lastServiceDate")]
    public DateOnly? LastServiceDate { get; set; }

    [JsonPropertyName("lastServiceMileage")]
    public int? LastServiceMileage { get; set; }

    [JsonPropertyName("estimatedMileage")]
    public int? EstimatedMileage { get; set; }

    [JsonPropertyName("serviceIntervals")]
    public ICollection<ServiceInterval>? ServiceIntervals { get; set; }
}
