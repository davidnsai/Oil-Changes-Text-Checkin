
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Core.Services.Interfaces;

public interface ICheckInSessionService : ISessionLoginService
{
    Task<List<CheckIn>> GetRecentCheckInsByLocationAsync(Guid locationId, CancellationToken cancellationToken = default);
}
