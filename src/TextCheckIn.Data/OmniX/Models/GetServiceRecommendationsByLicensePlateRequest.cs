namespace TextCheckIn.Data.OmniX.Models;

public class GetServiceRecommendationsByLicensePlateRequest
{
    public required Guid CheckInId { get; init; }
    public required string ClientLocationId { get; init; }

    public required string LicensePlate {  get; init; }

    public required string StateCode { get; init; }

    public int? Mileage { get; init; }
}