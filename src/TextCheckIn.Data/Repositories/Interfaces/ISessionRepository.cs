
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task<Sessions?> GetSessionAsync(Guid? sessionId);
        Task CreateSessionAsync(Sessions session);
        Task UpdateSessionAsync(Sessions session);
        Task DeleteSessionAsync(Guid sessionId);
    }
}
