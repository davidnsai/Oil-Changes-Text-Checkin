using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Core.Services;

public class RedisSessionLoginService : ISessionLoginService
{
    private readonly IRedisService _redisService;
    private readonly SessionConfiguration _sessionConfig;
    private readonly ILogger<RedisSessionLoginService> _logger;

    public RedisSessionLoginService(
        IRedisService redisService,
        IOptions<SessionConfiguration> sessionConfig,
        ILogger<RedisSessionLoginService> logger)
    {
        _redisService = redisService;
        _sessionConfig = sessionConfig.Value;
        _logger = logger;
    }

    public async Task<CheckInSession> LoginAsync(
        string phoneNumber,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(storeId))
            throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

        _logger.LogInformation("Redis login for phone: {PhoneNumber} at store: {StoreId}", phoneNumber, storeId);

        // Create Redis key from phone and store
        var sessionKey = $"session:{storeId}:{phoneNumber}";

        // Try to retrieve existing session
        var existingSession = await _redisService.GetAsync<CheckInSession>(sessionKey, cancellationToken);

        if (existingSession != null && !existingSession.IsExpired)
        {
            _logger.LogInformation("Found existing Redis session: {SessionId}", existingSession.Id);
            return existingSession;
        }

        // Create new session
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

        // Store in Redis with expiration
        var expiration = TimeSpan.FromMinutes(expirationMinutes);
        await _redisService.SetAsync(sessionKey, newSession, expiration, cancellationToken);

        _logger.LogInformation("Created new Redis session: {SessionId}, expires in {Minutes} minutes", 
            newSession.Id, expirationMinutes);

        return newSession;
    }
}
