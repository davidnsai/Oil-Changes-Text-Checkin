namespace TextCheckIn.Data.OmniX.Models;

public class GetServiceRecommendationsByVinRequest
{
    public required string ClientLocationId { get; init; }

    public required string Vin {  get; init; }

    public int? Mileage { get; init; }
}