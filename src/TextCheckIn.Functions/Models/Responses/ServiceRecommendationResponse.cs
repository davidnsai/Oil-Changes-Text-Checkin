namespace TextCheckIn.Functions.Models.Responses;

/// <summary>
/// Response DTO for service recommendations to avoid circular reference issues
/// </summary>
public class ServiceRecommendationResponse
{
    /// <summary>
    /// CheckInService ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Service ID
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Service UUID
    /// </summary>
    public Guid ServiceUuid { get; set; }

    /// <summary>
    /// Service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Service description
    /// </summary>
    public string? ServiceDescription { get; set; }

    /// <summary>
    /// Service price
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Whether customer selected this service
    /// </summary>
    public bool IsCustomerSelected { get; set; }

    /// <summary>
    /// Service interval in miles
    /// </summary>
    public int IntervalMiles { get; set; }

    /// <summary>
    /// Last service mileage
    /// </summary>
    public int? LastServiceMiles { get; set; }

    /// <summary>
    /// Mileage bucket
    /// </summary>
    public int MileageBucket { get; set; }
}
