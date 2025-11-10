using System;
using System.Threading.Tasks;
using TextCheckIn.Data.Entities;
using TextCheckIn.Core.Models.Domain;

namespace TextCheckIn.Core.Services.Interfaces
{
    public interface ISessionManagementService
    {
        Sessions? CurrentSession { get; }
        Task<Guid?> CreateNewSessionAsync();

        Task UpdateSessionAsync(Sessions session);

        Task UpdateSessionAsync(Action<SessionPayload> updateAction);

        Task<Sessions?> GetSessionAsync(Guid sessionId);
        SessionPayload? GetCurrentSessionPayload();
    }
}
