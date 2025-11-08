using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Responses;

/// <summary>
/// Standard API response wrapper for consistent response format
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Response data (present when successful)
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    /// <summary>
    /// Error message (present when unsuccessful)
    /// </summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>
    /// Additional error details (optional)
    /// </summary>
    [JsonPropertyName("errorDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ErrorDetails { get; set; }

    /// <summary>
    /// Indicates whether the request was successful or not
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Message (present when successful)
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    /// Unique request identifier for tracking and debugging
    /// </summary>
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Session ID
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
}

/// <summary>
/// Extension methods for creating standard API responses
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Create successful response with data
    /// </summary>
    public static ApiResponse<T> Success<T>(T data, string requestId)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create error response
    /// </summary>
    public static ApiResponse<T> Error<T>(string errorMessage, string requestId, object? errorDetails = null)
    {
        return new ApiResponse<T>
        {
            Error = errorMessage,
            ErrorDetails = errorDetails,
            Success = false,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }
}
