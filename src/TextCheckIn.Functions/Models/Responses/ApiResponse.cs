using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Responses;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    [JsonPropertyName("errorDetails")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ErrorDetails { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; set; }

    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
}

public static class ApiResponseExtensions
{
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
