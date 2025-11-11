using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Core.Services;

public class DatabaseSessionLoginService : ISessionLoginService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly SessionConfiguration _sessionConfig;
    private readonly ILogger<DatabaseSessionLoginService> _logger;

    public DatabaseSessionLoginService(
        ICheckInRepository checkInRepository,
        IOptions<SessionConfiguration> sessionConfig,
        ILogger<DatabaseSessionLoginService> logger)
    {
        _checkInRepository = checkInRepository;
        _sessionConfig = sessionConfig.Value;
        _logger = logger;
    }

    public Task<CheckInSession> LoginAsync(
        string phoneNumber,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(storeId))
            throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

        _logger.LogInformation("Database login for phone: {PhoneNumber} at store: {StoreId}", phoneNumber, storeId);

        // TODO: Implement database session retrieval when ICheckInRepository is extended
        // For now, return a new session placeholder
        // This will need to be updated when the repository methods are implemented:
        // - GetActiveSessionByPhoneAsync(phoneNumber, storeId, cancellationToken)
        // - SaveSessionAsync(session, cancellationToken)

        var expirationMinutes = _sessionConfig.DefaultExpirationMinutes > 0 
            ? _sessionConfig.DefaultExpirationMinutes 
            : 30;

        var newSession = new CheckInSession
        {
            Id = Guid.NewGuid().ToString(),
            StoreId = storeId,
            LicensePlate = string.Empty, // Will be set during check-in flow
            StateCode = string.Empty,    // Will be set during check-in flow
            CurrentStep = Shared.Enums.CheckInStep.LicensePlateSelection,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        _logger.LogInformation("Created new database session: {SessionId}, expires in {Minutes} minutes", 
            newSession.Id, expirationMinutes);

        // TODO: Save to database
        // await _checkInRepository.SaveSessionAsync(newSession, cancellationToken);

        return Task.FromResult(newSession);
    }
}
