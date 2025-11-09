using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Responses;

public class WebhookResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; set; }
}
