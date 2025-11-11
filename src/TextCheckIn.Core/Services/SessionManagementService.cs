using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Core.Services
{
    public class SessionManagementService : ISessionManagementService
    {
        public Sessions? CurrentSession { get; private set; }

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

        public async Task UpdateSessionAsync(Sessions session)
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
        /// Updates the current session's payload using a provided action.
        /// This method handles deserialization, update, and serialization of the payload.
        /// </summary>
        /// <param name="updateAction">The action to perform on the session payload.</param>
        public async Task UpdateSessionAsync(Action<SessionPayload> updateAction)
        {
            if (CurrentSession == null)
            {
                _logger.LogWarning("Cannot update session because there is no current session.");
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                SessionPayload payload;
                
                if (string.IsNullOrEmpty(CurrentSession.Payload))
                {
                    payload = new SessionPayload();
                }
                else
                {
                    try
                    {
                        payload = JsonSerializer.Deserialize<SessionPayload>(CurrentSession.Payload, options) ?? new SessionPayload();
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize session payload for session {SessionId}. Creating new payload.", CurrentSession.Id);
                        payload = new SessionPayload();
                    }
                }

                updateAction(payload);
                CurrentSession.Payload = JsonSerializer.Serialize(payload, options);
                await UpdateSessionAsync(CurrentSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session payload for session {SessionId}", CurrentSession.Id);
                throw;
            }
        }

        public async Task<Sessions?> GetSessionAsync(Guid sessionId)
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

        private bool IsSessionExpired(Sessions session)
        {
            return DateTime.UtcNow - session.LastActivity > _sessionTimeout;
        }

        private async Task<Guid> CreateNewSession()
        {
            var newSession = new Sessions
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

        public SessionPayload? GetCurrentSessionPayload()
        {
            if (CurrentSession == null)
            {
                _logger.LogDebug("No current session available to retrieve payload");
                return null;
            }

            // Check if the current session has expired
            if (IsSessionExpired(CurrentSession))
            {
                _logger.LogInformation(
                    "Current session {SessionId} has expired, cannot retrieve payload",
                    CurrentSession.Id);

                CurrentSession = null;
                return null;
            }

            if (string.IsNullOrEmpty(CurrentSession.Payload))
            {
                _logger.LogDebug("Current session {SessionId} has empty payload", CurrentSession.Id);
                return new SessionPayload();
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var payload = JsonSerializer.Deserialize<SessionPayload>(
                    CurrentSession.Payload, options);

                _logger.LogDebug("Payload retrieved for current session {SessionId}", CurrentSession.Id);
                return payload ?? new SessionPayload();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing payload for session {SessionId}", CurrentSession.Id);
                return null;
            }
        }
    }
}
