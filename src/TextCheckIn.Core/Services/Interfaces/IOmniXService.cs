using TextCheckIn.Data.Entities;
using TextCheckIn.Data.OmniX.Models;

namespace TextCheckIn.Core.Services.Interfaces;

public interface IOmniXService
{
    Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByCheckInUuidRequest request);

    Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification);
    Task SubmitWorkOrderAsync(Guid checkInUuid, CheckIn checkIn);
}

