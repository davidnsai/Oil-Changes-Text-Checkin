using System;
using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Core.Services.Interfaces
{
    public interface ISessionManagementService
    {
        CheckInSession? CurrentSession { get; }
        Task<Guid?> CreateNewSessionAsync();

        Task UpdateSessionAsync(CheckInSession session);

        Task<CheckInSession?> GetSessionAsync(Guid sessionId);
    }
}
