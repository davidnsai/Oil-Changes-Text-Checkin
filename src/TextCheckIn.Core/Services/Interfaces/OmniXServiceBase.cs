using Microsoft.Extensions.Logging;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Shared.Models;

namespace TextCheckIn.Core.Services.Interfaces;

public abstract class OmniXServiceBase
{
    private readonly ILogger<OmniXServiceBase> _logger;

    protected OmniXServiceBase(ILogger<OmniXServiceBase> logger)
    {
        _logger = logger;
    }

    public abstract Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByVinRequest request);

    public abstract Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByLicensePlateRequest request);

    public abstract Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification);
}
