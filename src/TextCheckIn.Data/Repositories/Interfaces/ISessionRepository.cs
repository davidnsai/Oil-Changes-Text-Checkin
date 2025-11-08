
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task<CheckInSession?> GetSessionAsync(Guid? sessionId);
        Task CreateSessionAsync(CheckInSession session);
        Task UpdateSessionAsync(CheckInSession session);
        Task DeleteSessionAsync(Guid sessionId);
    }
}
