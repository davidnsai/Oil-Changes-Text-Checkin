namespace TextCheckIn.Data.OmniX.Models;

public class GetServiceRecommendationsByCheckInUuidRequest
{
    public required Guid CheckInUuid { get; init; }

    public int? Mileage { get; init; }
}

