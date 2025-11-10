using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Core.Services
{
    public class OtpService : IOtpService
    {
        private readonly ILogger<OtpService> _logger;
        private readonly ISessionManagementService _sessionManagementService;

        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPTS = 3;
        private const int BASE_COOLDOWN_MINUTES = 2; // Initial cooldown: 2 minutes

        public OtpService(
            ILogger<OtpService> logger,
            ISessionManagementService sessionManagementService)
        {
            _logger = logger;
            _sessionManagementService = sessionManagementService;
        }

        private SessionPayload GetSessionPayload()
        {
            if (string.IsNullOrEmpty(_sessionManagementService.CurrentSession?.Payload))
            {
                return new SessionPayload();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            return JsonSerializer.Deserialize<SessionPayload>(
                _sessionManagementService.CurrentSession.Payload, options) ?? new SessionPayload();
        }

        private async Task SaveSessionPayloadAsync(SessionPayload payload)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _sessionManagementService.CurrentSession!.Payload = JsonSerializer.Serialize(payload, options);
            await _sessionManagementService.UpdateSessionAsync(_sessionManagementService.CurrentSession);
        }

        public async Task<string> GenerateOtpAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                throw new InvalidOperationException("No active session found. OTP generation requires an active session.");
            }

            var payload = GetSessionPayload();

            // Check for existing cooldown data
            if (payload.OtpCooldown != null &&
                payload.OtpCooldown.PhoneNumber == phoneNumber &&
                DateTime.UtcNow < payload.OtpCooldown.CooldownUntil)
            {
                var remainingSeconds = (int)(payload.OtpCooldown.CooldownUntil - DateTime.UtcNow).TotalSeconds;
                _logger.LogWarning("Phone number {PhoneNumber} is in cooldown for {RemainingSeconds} more seconds",
                    phoneNumber, remainingSeconds);
                throw new InvalidOperationException($"Please wait {remainingSeconds} seconds before requesting another OTP");
            }

            // Check for duplicate OTP request (same phone number, OTP still valid)
            if (payload.OtpData != null &&
                payload.OtpData.PhoneNumber == phoneNumber &&
                DateTime.UtcNow < payload.OtpData.OtpExpiry)
            {
                var remainingSeconds = (int)(payload.OtpData.OtpExpiry - DateTime.UtcNow).TotalSeconds;
                _logger.LogWarning("Duplicate OTP request for phone number {PhoneNumber}. Existing OTP still valid for {RemainingSeconds} seconds",
                    phoneNumber, remainingSeconds);
                throw new InvalidOperationException($"An OTP was already sent. Please wait {remainingSeconds} seconds before requesting a new one");
            }

            // Get send count to calculate cooldown
            int sendCount = payload.OtpSendCount?.Count ?? 0;

            // Generate a random 6-digit OTP
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));
            var otpCode = (randomValue % 900000 + 100000).ToString(); // Ensures 6 digits

            var now = DateTime.UtcNow;
            var otpExpiry = now.AddMinutes(OTP_EXPIRY_MINUTES);

            // Store OTP data in session payload
            payload.OtpData = new OtpData
            {
                PhoneNumber = phoneNumber,
                OtpCode = otpCode,
                OtpGenerated = now,
                OtpExpiry = otpExpiry,
                Attempts = 0 // Start at 0, will increment on first validation attempt
            };

            // Increment send count
            sendCount++;
            payload.OtpSendCount = new OtpSendCount
            {
                PhoneNumber = phoneNumber,
                Count = sendCount,
                LastSendTime = now
            };

            // Calculate cooldown: 2 minutes * 2^(sendCount-1)
            // sendCount=1: 2 min, sendCount=2: 4 min, sendCount=3: 8 min, etc.
            var cooldownMinutes = BASE_COOLDOWN_MINUTES * Math.Pow(2, sendCount - 1);
            var nextCooldownUntil = now.AddMinutes(cooldownMinutes);

            payload.OtpCooldown = new OtpCooldown
            {
                PhoneNumber = phoneNumber,
                CooldownUntil = nextCooldownUntil,
                SendCount = sendCount
            };

            await SaveSessionPayloadAsync(payload);

            _logger.LogInformation("Generated OTP for phone number {PhoneNumber}. Send count: {SendCount}, Next cooldown: {CooldownMinutes} minutes",
                phoneNumber, sendCount, cooldownMinutes);

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                _logger.LogWarning("No active session found for OTP validation");
                return false;
            }

            var payload = GetSessionPayload();

            if (payload.OtpData == null)
            {
                _logger.LogWarning("No OTP found for phone number {PhoneNumber}", phoneNumber);
                return false;
            }

            // Check if OTP is for the correct phone number
            if (payload.OtpData.PhoneNumber != phoneNumber)
            {
                _logger.LogWarning("OTP phone number mismatch for {PhoneNumber}", phoneNumber);
                return false;
            }

            // Check if OTP has expired
            if (DateTime.UtcNow > payload.OtpData.OtpExpiry)
            {
                _logger.LogWarning("OTP has expired for phone number {PhoneNumber}", phoneNumber);
                await ClearOtpAsync(phoneNumber);
                return false;
            }

            // Check if max attempts exceeded
            if (payload.OtpData.Attempts >= MAX_ATTEMPTS)
            {
                _logger.LogWarning("Max OTP attempts exceeded for phone number {PhoneNumber}", phoneNumber);
                return false;
            }

            // Increment attempts
            payload.OtpData.Attempts++;
            await SaveSessionPayloadAsync(payload);

            // Validate OTP
            if (payload.OtpData.OtpCode == otpCode)
            {
                _logger.LogInformation("OTP validated successfully for phone number {PhoneNumber}", phoneNumber);
                // Clear OTP after successful validation
                await ClearOtpAsync(phoneNumber);
                return true;
            }

            _logger.LogWarning(
                "Invalid OTP attempt for phone number {PhoneNumber}. Attempt {Attempt}/{MaxAttempts}",
                phoneNumber,
                payload.OtpData.Attempts,
                MAX_ATTEMPTS);

            return false;
        }

        public Task<int?> GetRemainingAttemptsAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult<int?>(null);
            }

            var payload = GetSessionPayload();

            if (payload.OtpData == null || payload.OtpData.PhoneNumber != phoneNumber)
            {
                return Task.FromResult<int?>(null);
            }

            var remaining = MAX_ATTEMPTS - payload.OtpData.Attempts;
            return Task.FromResult<int?>(remaining > 0 ? remaining : 0);
        }

        public async Task ClearOtpAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                _logger.LogWarning("No active session found for OTP clearing");
                return;
            }

            var payload = GetSessionPayload();

            if (payload.OtpData != null)
            {
                payload.OtpData = null;
                await SaveSessionPayloadAsync(payload);
            }

            _logger.LogInformation("Cleared OTP data for phone number {PhoneNumber}", phoneNumber);
        }

        public Task<bool> IsInCooldownAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult(false);
            }

            var payload = GetSessionPayload();

            if (payload.OtpCooldown == null || payload.OtpCooldown.PhoneNumber != phoneNumber)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(DateTime.UtcNow < payload.OtpCooldown.CooldownUntil);
        }

        public Task<int?> GetCooldownRemainingSecondsAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult<int?>(null);
            }

            var payload = GetSessionPayload();

            if (payload.OtpCooldown == null || payload.OtpCooldown.PhoneNumber != phoneNumber)
            {
                return Task.FromResult<int?>(null);
            }

            var remaining = (payload.OtpCooldown.CooldownUntil - DateTime.UtcNow).TotalSeconds;
            if (remaining > 0)
            {
                return Task.FromResult<int?>((int)Math.Ceiling(remaining));
            }

            return Task.FromResult<int?>(null);
        }
    }
}
