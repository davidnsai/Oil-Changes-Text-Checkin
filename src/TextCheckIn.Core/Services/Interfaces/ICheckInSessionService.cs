
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Core.Services.Interfaces;

/// <summary>
/// Service for managing customer check-in sessions
/// </summary>
public interface ICheckInSessionService : ISessionLoginService
{
    /// <summary>
    /// Gets recent unprocessed check-ins for a specific location and returns vehicle license plates
    /// </summary>
    /// <param name="locationId">The location ID to filter check-ins</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of vehicle license plates from recent unprocessed check-ins</returns>
    Task<List<CheckIn>> GetRecentCheckInsByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);
}
