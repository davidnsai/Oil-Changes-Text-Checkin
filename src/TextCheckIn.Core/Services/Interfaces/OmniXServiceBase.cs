using Microsoft.Extensions.Logging;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Shared.Models;

namespace TextCheckIn.Core.Services.Interfaces;

/// <summary>
/// Service for interacting with omniX APIs and processing vehicle recommendations
/// </summary>
public abstract class OmniXServiceBase
{
    /// <summary>
    /// Logger for the OmniX service
    /// </summary>
    private readonly ILogger<OmniXServiceBase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmniXServiceBase"/> class
    /// </summary>
    /// <param name="logger">The logger for this service</param>
    protected OmniXServiceBase(ILogger<OmniXServiceBase> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets service recommendations by vehicle VIN
    /// </summary>
    /// <param name="request">The request containing the VIN and optional parameters</param>
    /// <returns>Service recommendations for the specified vehicle or null if not found</returns>
    public abstract Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByVinRequest request);

    /// <summary>
    /// Gets service recommendations by vehicle license plate
    /// </summary>
    /// <param name="request">The request containing the license plate and optional parameters</param>
    /// <returns>Service recommendations for the specified vehicle or null if not found</returns>
    public abstract Task<List<CheckInService>> GetServiceRecommendationAsync(GetServiceRecommendationsByLicensePlateRequest request);

    /// <summary>
    /// Processes an incoming service recommendation notification from omniX
    /// </summary>
    /// <param name="notification">The notification data containing service recommendations</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public abstract Task ProcessIncomingServiceRecommendationAsync(ServiceRecommendation notification);
}
