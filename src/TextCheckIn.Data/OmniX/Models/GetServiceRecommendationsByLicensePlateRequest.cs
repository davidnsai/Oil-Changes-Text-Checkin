namespace TextCheckIn.Data.OmniX.Models;

public class GetServiceRecommendationsByLicensePlateRequest : BaseGetServiceRecommendationsRequest
{
    public required Guid CheckInId { get; init; }

    public required string LicensePlate {  get; init; }

    public required string StateCode { get; init; }
}