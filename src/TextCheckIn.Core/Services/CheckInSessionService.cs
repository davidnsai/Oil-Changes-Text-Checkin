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
        private readonly IOmniXService _omniXService;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ILogger<CheckInSessionService> _logger;

        public CheckInSessionService(
            ICheckInRepository checkInRepository,
            IOmniXService omniXService,
            ISessionManagementService sessionManagementService,
            ILogger<CheckInSessionService> logger)
        {
            _checkInRepository = checkInRepository;
            _omniXService = omniXService;
            _sessionManagementService = sessionManagementService;
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

        public async Task<CheckIn> SubmitCheckInAsync(
            Guid checkInUuid,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var checkIn = await _checkInRepository.GetCheckInByUuidAsync(checkInUuid);
                if (checkIn == null || _sessionManagementService.CurrentSession == null || _sessionManagementService.CurrentSession.CustomerId == null)
                {
                    throw new Exception("Check-in not found");
                }
                checkIn.IsProcessed = true;
                checkIn.CustomerId = _sessionManagementService.CurrentSession.CustomerId;
                await _checkInRepository.UpdateCheckInAsync(checkIn);
                await _omniXService.SubmitWorkOrderAsync(checkInUuid, checkIn);
                return checkIn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting check-in: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
