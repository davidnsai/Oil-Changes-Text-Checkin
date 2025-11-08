using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Core.Services
{
    /// <summary>
    /// Service for managing user sessions.
    /// </summary>
    public class SessionManagementService : ISessionManagementService
    {
        /// <summary>
        /// The current session.
        /// </summary>
        public CheckInSession? CurrentSession { get; private set; }

        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<SessionManagementService> _logger;

        // configurable session timeout
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Initializes a new instance of the SessionManagementService class with the specified session repository and
        /// logger.
        /// </summary>
        /// <param name="sessionRepository">The repository used to store and retrieve session data</param>
        /// <param name="logger">The logger used to record diagnostic and operational information for the service</param>
        public SessionManagementService(
            ISessionRepository sessionRepository,
            ILogger<SessionManagementService> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new session.
        /// </summary>
        /// <returns>Session ID.</returns>
        public async Task<Guid?> CreateNewSessionAsync()
        {
            try
            {             
               // create new session
                return await CreateNewSession();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting or creating session");
                throw;
            }
        }

        /// <summary>
        /// Updates the current session in the repository (e.g. when data changes).
        /// </summary>
        public async Task UpdateSessionAsync(CheckInSession session)
        {
            try
            {
                session.LastActivity = DateTime.UtcNow;
                await _sessionRepository.UpdateSessionAsync(session);
                _logger.LogDebug("Session {SessionId} updated successfully.", session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", session.Id);
                throw;
            }
        }


        /// <summary>
        /// Gets an existing session by ID without creating a new one.
        /// Checks expiry and updates activity if valid.
        /// </summary>
        /// <param name="sessionId">Session ID to retrieve</param>
        /// <returns>Session if found and valid, null otherwise</returns>
        public async Task<CheckInSession?> GetSessionAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionRepository.GetSessionAsync(sessionId);
                
                if (session == null)
                {
                    _logger.LogInformation("Session {SessionId} not found", sessionId);
                    return null;
                }

                // Check if expired
                if (IsSessionExpired(session))
                {
                    _logger.LogInformation(
                        "Session {SessionId} has expired (last activity: {LastActivity}). User must start a new session.",
                        session.Id,
                        session.LastActivity);

                    await _sessionRepository.DeleteSessionAsync(session.Id);
                    return null;
                }

                // Update activity and set as current session
                session.LastActivity = DateTime.UtcNow;
                await UpdateSessionAsync(session);
                CurrentSession = session;
                
                _logger.LogDebug("Session {SessionId} retrieved and validated", sessionId);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting session {SessionId}", sessionId);
                return null;
            }
        }

        /// <summary>
        /// Checks if a session is expired.
        /// </summary>
        private bool IsSessionExpired(CheckInSession session)
        {
            return DateTime.UtcNow - session.LastActivity > _sessionTimeout;
        }

        /// <summary>
        /// Creates a new session.
        /// </summary>
        private async Task<Guid> CreateNewSession()
        {
            var newSession = new CheckInSession
            {
                Id = Guid.NewGuid(),
                LastActivity = DateTime.UtcNow,
                Payload = string.Empty
            };

            await _sessionRepository.CreateSessionAsync(newSession);
            CurrentSession = newSession;

            _logger.LogInformation("New session created with ID {SessionId}", newSession.Id);

            return newSession.Id;
        }

    }
}
