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
    /// <summary>
    /// Service responsible for managing check-in sessions and related operations.
    /// </summary>
    public class CheckInSessionService : ICheckInSessionService
    {
        private readonly ICheckInRepository _checkInRepository;
        private readonly ISessionLoginService _sessionLoginService;
        private readonly SessionConfiguration _sessionConfig;
        private readonly ILogger<CheckInSessionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInSessionService"/> class.
        /// </summary>
        /// <param name="checkInRepository">The repository for accessing check-in data.</param>
        /// <param name="sessionLoginService">The session login service (Redis or Database based)</param>
        /// <param name="sessionConfig">Session configuration settings</param>
        /// <param name="logger">Logger instance.</param>
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

        /// <summary>
        /// Gets recent unprocessed check-ins for a specific location and returns vehicle license plates
        /// </summary>
        /// <param name="locationId">The location ID to filter check-ins</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of vehicle license plates from recent unprocessed check-ins</returns>
        public Task<List<CheckIn>> GetRecentCheckInsByLocationAsync(
            Guid locationId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving recent unprocessed check-ins for location: {LocationId}", locationId);

            var checkIns = _checkInRepository.GetRecentUnprocessedCheckInsByLocation(locationId);
            
            return Task.FromResult(checkIns);
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Login using Redis storage
        /// </summary>
        private async Task<DomainCheckInSession> LoginWithRedisAsync(
            string phoneNumber,
            string storeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Routing to Redis session login service");
            return await _sessionLoginService.LoginAsync(phoneNumber, storeId, cancellationToken);
        }

        /// <summary>
        /// Login using Database storage
        /// </summary>
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
