using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Core.Services
{
    /// <summary>
    /// Service for managing OTP (One-Time Password) verification
    /// </summary>
    public class OtpService : IOtpService
    {
        private readonly ILogger<OtpService> _logger;
        private readonly ISessionManagementService _sessionManagementService;

        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPTS = 3;
        private const int BASE_COOLDOWN_MINUTES = 2; // Initial cooldown: 2 minutes

        /// <summary>
        /// Constructor for the OtpService
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sessionManagementService"></param>
        public OtpService(
            ILogger<OtpService> logger,
            ISessionManagementService sessionManagementService)
        {
            _logger = logger;
            _sessionManagementService = sessionManagementService;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateOtpAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                throw new InvalidOperationException("No active session found. OTP generation requires an active session.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            // Parse existing payload or create new
            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            // Check for existing cooldown data
            if (payloadData.ContainsKey("otpCooldown"))
            {
                var cooldownData = payloadData["otpCooldown"] as JsonElement?;
                if (cooldownData.HasValue)
                {
                    var cooldownElement = cooldownData.Value;

                    // Check if cooldown is for the correct phone number
                    if (cooldownElement.TryGetProperty("phoneNumber", out var cooldownPhoneElement) &&
                        cooldownPhoneElement.GetString() == phoneNumber)
                    {
                        // Check if still in cooldown period
                        if (cooldownElement.TryGetProperty("cooldownUntil", out var cooldownUntilElement))
                        {
                            if (DateTime.TryParse(cooldownUntilElement.GetString(), out var cooldownUntil) &&
                                DateTime.UtcNow < cooldownUntil)
                            {
                                var remainingSeconds = (int)(cooldownUntil - DateTime.UtcNow).TotalSeconds;
                                _logger.LogWarning("Phone number {PhoneNumber} is in cooldown for {RemainingSeconds} more seconds",
                                    phoneNumber, remainingSeconds);
                                throw new InvalidOperationException($"Please wait {remainingSeconds} seconds before requesting another OTP");
                            }
                        }
                    }
                }
            }

            // Check for duplicate OTP request (same phone number, OTP still valid)
            if (payloadData.ContainsKey("otpData"))
            {
                var existingOtpData = payloadData["otpData"] as JsonElement?;
                if (existingOtpData.HasValue)
                {
                    var otpElement = existingOtpData.Value;

                    // Check if OTP is for the same phone number
                    if (otpElement.TryGetProperty("phoneNumber", out var phoneElement) &&
                        phoneElement.GetString() == phoneNumber)
                    {
                        // Check if OTP is still valid (not expired)
                        if (otpElement.TryGetProperty("otpExpiry", out var expiryElement))
                        {
                            if (DateTime.TryParse(expiryElement.GetString(), out var expiry) &&
                                DateTime.UtcNow < expiry)
                            {
                                var remainingSeconds = (int)(expiry - DateTime.UtcNow).TotalSeconds;
                                _logger.LogWarning("Duplicate OTP request for phone number {PhoneNumber}. Existing OTP still valid for {RemainingSeconds} seconds",
                                    phoneNumber, remainingSeconds);
                                throw new InvalidOperationException($"An OTP was already sent. Please wait {remainingSeconds} seconds before requesting a new one");
                            }
                        }
                    }
                }
            }

            // Get send count to calculate cooldown
            int sendCount = 0;
            if (payloadData.ContainsKey("otpSendCount"))
            {
                var sendCountData = payloadData["otpSendCount"] as JsonElement?;
                if (sendCountData.HasValue && sendCountData.Value.TryGetProperty("count", out var countElement))
                {
                    sendCount = countElement.TryGetInt32(out var count) ? count : 0;
                }
            }

            // Generate a random 6-digit OTP
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));
            var otpCode = (randomValue % 900000 + 100000).ToString(); // Ensures 6 digits

            var now = DateTime.UtcNow;
            var otpExpiry = now.AddMinutes(OTP_EXPIRY_MINUTES);

            // Store OTP data in session payload
            var otpData = new
            {
                phoneNumber,
                otpCode,
                otpGenerated = now,
                otpExpiry = otpExpiry,
                attempts = 0 // Start at 0, will increment on first validation attempt
            };

            payloadData["otpData"] = otpData;

            // Increment send count
            sendCount++;
            payloadData["otpSendCount"] = new
            {
                phoneNumber,
                count = sendCount,
                lastSendTime = now
            };

            // Calculate cooldown: 2 minutes * 2^(sendCount-1)
            // sendCount=1: 2 min, sendCount=2: 4 min, sendCount=3: 8 min, etc.
            var cooldownMinutes = BASE_COOLDOWN_MINUTES * Math.Pow(2, sendCount - 1);
            var nextCooldownUntil = now.AddMinutes(cooldownMinutes);

            payloadData["otpCooldown"] = new
            {
                phoneNumber,
                cooldownUntil = nextCooldownUntil,
                sendCount = sendCount
            };

            _sessionManagementService.CurrentSession.Payload = JsonSerializer.Serialize(payloadData, options);
            await _sessionManagementService.UpdateSessionAsync(_sessionManagementService.CurrentSession);

            _logger.LogInformation("Generated OTP for phone number {PhoneNumber}. Send count: {SendCount}, Next cooldown: {CooldownMinutes} minutes",
                phoneNumber, sendCount, cooldownMinutes);

            return otpCode;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                _logger.LogWarning("No active session found for OTP validation");
                return false;
            }

            // Get OTP data from session payload
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            if (!payloadData.ContainsKey("otpData"))
            {
                _logger.LogWarning("No OTP found for phone number {PhoneNumber}", phoneNumber);
                return false;
            }

            var otpData = payloadData["otpData"] as JsonElement?;
            if (!otpData.HasValue)
            {
                _logger.LogWarning("Invalid OTP data in session for phone number {PhoneNumber}", phoneNumber);
                return false;
            }

            var otpElement = otpData.Value;

            // Check if OTP is for the correct phone number
            if (!otpElement.TryGetProperty("phoneNumber", out var phoneNumberElement) ||
                phoneNumberElement.GetString() != phoneNumber)
            {
                _logger.LogWarning("OTP phone number mismatch for {PhoneNumber}", phoneNumber);
                return false;
            }

            // Check if OTP has expired
            if (otpElement.TryGetProperty("otpExpiry", out var expiryElement))
            {
                if (DateTime.TryParse(expiryElement.GetString(), out var expiry) && DateTime.UtcNow > expiry)
                {
                    _logger.LogWarning("OTP has expired for phone number {PhoneNumber}", phoneNumber);
                    await ClearOtpAsync(phoneNumber);
                    return false;
                }
            }

            // Get current attempts
            var attempts = 0;
            if (otpElement.TryGetProperty("attempts", out var attemptsElement) &&
                attemptsElement.TryGetInt32(out var parsedAttempts))
            {
                attempts = parsedAttempts;
            }

            // Check if max attempts exceeded
            if (attempts >= MAX_ATTEMPTS)
            {
                _logger.LogWarning("Max OTP attempts exceeded for phone number {PhoneNumber}", phoneNumber);
                return false;
            }

            // Increment attempts
            attempts++;

            // Update the OTP data with new attempts count
            var updatedOtpData = new
            {
                phoneNumber = otpElement.GetProperty("phoneNumber").GetString(),
                otpCode = otpElement.GetProperty("otpCode").GetString(),
                otpGenerated = otpElement.GetProperty("otpGenerated").GetDateTime(),
                otpExpiry = otpElement.GetProperty("otpExpiry").GetDateTime(),
                attempts = attempts
            };

            payloadData["otpData"] = updatedOtpData;
            _sessionManagementService.CurrentSession.Payload = JsonSerializer.Serialize(payloadData, options);
            await _sessionManagementService.UpdateSessionAsync(_sessionManagementService.CurrentSession);

            // Validate OTP
            var storedOtp = otpElement.TryGetProperty("otpCode", out var otpCodeElement) ? otpCodeElement.GetString() : null;
            if (storedOtp == otpCode)
            {
                _logger.LogInformation("OTP validated successfully for phone number {PhoneNumber}", phoneNumber);
                // Clear OTP after successful validation
                await ClearOtpAsync(phoneNumber);
                return true;
            }

            _logger.LogWarning(
                "Invalid OTP attempt for phone number {PhoneNumber}. Attempt {Attempt}/{MaxAttempts}",
                phoneNumber,
                attempts,
                MAX_ATTEMPTS);

            return false;
        }

        /// <inheritdoc/>
        public Task<int?> GetRemainingAttemptsAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult<int?>(null);
            }

            // Get OTP data from session payload
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            if (!payloadData.ContainsKey("otpData"))
            {
                return Task.FromResult<int?>(null);
            }

            var otpData = payloadData["otpData"] as JsonElement?;
            if (!otpData.HasValue)
            {
                return Task.FromResult<int?>(null);
            }

            var otpElement = otpData.Value;

            // Check if OTP is for the correct phone number
            if (!otpElement.TryGetProperty("phoneNumber", out var phoneNumberElement) ||
                phoneNumberElement.GetString() != phoneNumber)
            {
                return Task.FromResult<int?>(null);
            }

            // Get current attempts
            var attempts = 0;
            if (otpElement.TryGetProperty("attempts", out var attemptsElement) &&
                attemptsElement.TryGetInt32(out var parsedAttempts))
            {
                attempts = parsedAttempts;
            }

            var remaining = MAX_ATTEMPTS - attempts;
            return Task.FromResult<int?>(remaining > 0 ? remaining : 0);
        }

        /// <inheritdoc/>
        public async Task ClearOtpAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                _logger.LogWarning("No active session found for OTP clearing");
                return;
            }

            // Clear OTP data from session
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            if (payloadData.ContainsKey("otpData"))
            {
                payloadData.Remove("otpData");
                _sessionManagementService.CurrentSession.Payload = JsonSerializer.Serialize(payloadData, options);
                await _sessionManagementService.UpdateSessionAsync(_sessionManagementService.CurrentSession);
            }

            _logger.LogInformation("Cleared OTP data for phone number {PhoneNumber}", phoneNumber);
        }

        /// <inheritdoc/>
        public Task<bool> IsInCooldownAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult(false);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            if (!payloadData.ContainsKey("otpCooldown"))
            {
                return Task.FromResult(false);
            }

            var cooldownData = payloadData["otpCooldown"] as JsonElement?;
            if (!cooldownData.HasValue)
            {
                return Task.FromResult(false);
            }

            var cooldownElement = cooldownData.Value;

            // Check if cooldown is for the correct phone number
            if (!cooldownElement.TryGetProperty("phoneNumber", out var phoneElement) ||
                phoneElement.GetString() != phoneNumber)
            {
                return Task.FromResult(false);
            }

            // Check if still in cooldown period
            if (cooldownElement.TryGetProperty("cooldownUntil", out var cooldownUntilElement))
            {
                if (DateTime.TryParse(cooldownUntilElement.GetString(), out var cooldownUntil))
                {
                    return Task.FromResult(DateTime.UtcNow < cooldownUntil);
                }
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<int?> GetCooldownRemainingSecondsAsync(string phoneNumber)
        {
            // Check if session exists
            if (_sessionManagementService.CurrentSession == null)
            {
                return Task.FromResult<int?>(null);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var payloadData = string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload, options) ?? new Dictionary<string, object>();

            if (!payloadData.ContainsKey("otpCooldown"))
            {
                return Task.FromResult<int?>(null);
            }

            var cooldownData = payloadData["otpCooldown"] as JsonElement?;
            if (!cooldownData.HasValue)
            {
                return Task.FromResult<int?>(null);
            }

            var cooldownElement = cooldownData.Value;

            // Check if cooldown is for the correct phone number
            if (!cooldownElement.TryGetProperty("phoneNumber", out var phoneElement) ||
                phoneElement.GetString() != phoneNumber)
            {
                return Task.FromResult<int?>(null);
            }

            // Check if still in cooldown period
            if (cooldownElement.TryGetProperty("cooldownUntil", out var cooldownUntilElement))
            {
                if (DateTime.TryParse(cooldownUntilElement.GetString(), out var cooldownUntil))
                {
                    var remaining = (cooldownUntil - DateTime.UtcNow).TotalSeconds;
                    if (remaining > 0)
                    {
                        return Task.FromResult<int?>((int)Math.Ceiling(remaining));
                    }
                }
            }

            return Task.FromResult<int?>(null);
        }
    }
}
