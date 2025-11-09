namespace TextCheckIn.Data.OmniX.Models;

public class GetServiceRecommendationsByVinRequest : BaseGetServiceRecommendationsRequest
{
    public required string Vin {  get; init; }
}