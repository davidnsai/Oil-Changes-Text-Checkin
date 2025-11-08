using System;
using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Core.Services.Interfaces
{
    /// <summary>
    /// Service Interface for managing user sessions
    /// </summary>
    public interface ISessionManagementService
    {
        /// <summary>
        /// The current session
        /// </summary>
        CheckInSession? CurrentSession { get; }
        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <returns>Session ID</returns>
        Task<Guid?> CreateNewSessionAsync();

        /// <summary>
        /// Updates the current session with new data
        /// </summary>
        /// <param name="session">The session to update</param>
        Task UpdateSessionAsync(CheckInSession session);

        /// <summary>
        /// Gets an existing session by ID without creating a new one
        /// </summary>
        /// <param name="sessionId">Session ID to retrieve</param>
        /// <returns>Session if found and valid, null otherwise</returns>
        Task<CheckInSession?> GetSessionAsync(Guid sessionId);
    }
}
