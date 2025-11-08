using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Responses;

/// <summary>
/// Response model for webhook acknowledgments
/// </summary>
public class WebhookResponse
{
    /// <summary>
    /// Whether the webhook was processed successfully
    /// </summary>
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// Unique request identifier for tracking
    /// </summary>
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; set; }
}
