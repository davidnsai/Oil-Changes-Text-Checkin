using System;
using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    /// <summary>
    /// Service for managing OTP (One-Time Password) verification
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generates and stores a new OTP for the given phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to generate OTP for</param>
        /// <returns>The generated OTP code</returns>
        /// <exception cref="InvalidOperationException">Thrown when cooldown period is still active</exception>
        Task<string> GenerateOtpAsync(string phoneNumber);

        /// <summary>
        /// Validates an OTP code for the given phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate OTP for</param>
        /// <param name="otpCode">The OTP code to validate</param>
        /// <returns>True if OTP is valid, false otherwise</returns>
        Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode);

        /// <summary>
        /// Gets the number of remaining attempts for OTP validation
        /// </summary>
        /// <param name="phoneNumber">The phone number to check attempts for</param>
        /// <returns>Number of remaining attempts, or null if no OTP exists</returns>
        Task<int?> GetRemainingAttemptsAsync(string phoneNumber);

        /// <summary>
        /// Clears OTP data for a phone number (after successful validation or expiry)
        /// </summary>
        /// <param name="phoneNumber">The phone number to clear OTP for</param>
        Task ClearOtpAsync(string phoneNumber);

        /// <summary>
        /// Checks if a phone number is currently in cooldown period
        /// </summary>
        /// <param name="phoneNumber">The phone number to check</param>
        /// <returns>True if in cooldown, false otherwise</returns>
        Task<bool> IsInCooldownAsync(string phoneNumber);

        /// <summary>
        /// Gets the remaining cooldown time in seconds for a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to check</param>
        /// <returns>Remaining cooldown time in seconds, or null if not in cooldown</returns>
        Task<int?> GetCooldownRemainingSecondsAsync(string phoneNumber);
    }
}
