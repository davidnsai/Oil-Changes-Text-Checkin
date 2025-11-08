using TextCheckIn.Data.OmniX.Models;

namespace TextCheckIn.Data.OmniX.Repositories.Abstractions;

public interface IOmniXRepository
{
    Task<ServiceRecommendation?> GetServiceRecommendationsAsync(GetServiceRecommendationsByVinRequest request);
    Task<ServiceRecommendation?> GetServiceRecommendationsAsync(GetServiceRecommendationsByLicensePlateRequest request);
}