namespace TextCheckIn.Data.OmniX.Models;

/// <summary>
/// Base class for service recommendation requests with common properties
/// </summary>
public abstract class BaseGetServiceRecommendationsRequest
{
    public required string ClientLocationId { get; init; }

    public int? Mileage { get; init; }
}
