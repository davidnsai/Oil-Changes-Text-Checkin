using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Services.Interfaces;
using DomainCheckInSession = TextCheckIn.Core.Models.Domain.CheckInSession;

namespace TextCheckIn.Core.Services
{
    public class CheckInSessionService : ICheckInSessionService
    {
        private readonly ICheckInRepository _checkInRepository;
        private readonly ISessionLoginService _sessionLoginService;
        private readonly SessionConfiguration _sessionConfig;
        private readonly ILogger<CheckInSessionService> _logger;

        public CheckInSessionService(
            ICheckInRepository checkInRepository,
            ISessionLoginService sessionLoginService,
            IOptions<SessionConfiguration> sessionConfig,
            ILogger<CheckInSessionService> logger)
        {
            _checkInRepository = checkInRepository;
            _sessionLoginService = sessionLoginService;
            _sessionConfig = sessionConfig.Value;
            _logger = logger;
        }

        public Task<List<CheckIn>> GetRecentCheckInsByLocationAsync(
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving recent unprocessed check-ins for location: {LocationId}", locationId);

            var checkIns = _checkInRepository.GetRecentUnprocessedCheckInsByLocation(locationId);
            
            return Task.FromResult(checkIns);
        }

        public async Task<DomainCheckInSession> LoginAsync(
            string phoneNumber,
            string storeId,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(storeId))
                throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

            _logger.LogInformation("Processing login for phone: {PhoneNumber} at store: {StoreId} using {StorageType} storage", 
                phoneNumber, storeId, _sessionConfig.StorageType);

            // Route to appropriate login method based on configuration
            return _sessionConfig.StorageType switch
            {
                SessionStorageType.Redis => await LoginWithRedisAsync(phoneNumber, storeId, cancellationToken),
                SessionStorageType.Database => await LoginWithDatabaseAsync(phoneNumber, storeId, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown session storage type: {_sessionConfig.StorageType}")
            };
        }

        private async Task<DomainCheckInSession> LoginWithRedisAsync(
            string phoneNumber,
            string storeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Routing to Redis session login service");
            return await _sessionLoginService.LoginAsync(phoneNumber, storeId, cancellationToken);
        }

        private async Task<DomainCheckInSession> LoginWithDatabaseAsync(
            string phoneNumber,
            string storeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Routing to Database session login service");
            return await _sessionLoginService.LoginAsync(phoneNumber, storeId, cancellationToken);
        }

    }
}
