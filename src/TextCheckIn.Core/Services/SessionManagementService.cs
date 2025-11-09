using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Core.Services
{
    public class SessionManagementService : ISessionManagementService
    {
        public CheckInSession? CurrentSession { get; private set; }

        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<SessionManagementService> _logger;

        // configurable session timeout
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);

        public SessionManagementService(
            ISessionRepository sessionRepository,
            ILogger<SessionManagementService> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

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

        private bool IsSessionExpired(CheckInSession session)
        {
            return DateTime.UtcNow - session.LastActivity > _sessionTimeout;
        }

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
