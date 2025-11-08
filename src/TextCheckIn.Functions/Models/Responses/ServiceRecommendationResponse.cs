namespace TextCheckIn.Functions.Models.Responses;

public class ServiceRecommendationResponse
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public Guid ServiceUuid { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string? ServiceDescription { get; set; }

    public decimal? Price { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public bool IsCustomerSelected { get; set; }

    public int IntervalMiles { get; set; }

    public int? LastServiceMiles { get; set; }

    public int MileageBucket { get; set; }
}
