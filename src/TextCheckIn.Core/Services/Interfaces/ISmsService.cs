using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    /// <summary>
    /// Service for sending SMS messages
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Sends an SMS message to the specified phone number
        /// </summary>
        /// <param name="phoneNumber">The recipient's phone number</param>
        /// <param name="message">The SMS message content</param>
        /// <param name="firstName">Optional first name of recipient</param>
        /// <param name="lastName">Optional last name of recipient</param>
        /// <returns>True if message was sent successfully, false otherwise</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message, string firstName = "", string lastName = "");

        /// <summary>
        /// Sends an OTP verification code via SMS
        /// </summary>
        /// <param name="phoneNumber">The recipient's phone number</param>
        /// <param name="otpCode">The OTP code to send</param>
        /// <param name="firstName">Optional first name of recipient</param>
        /// <param name="lastName">Optional last name of recipient</param>
        /// <returns>True if OTP was sent successfully, false otherwise</returns>
        Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string firstName = "", string lastName = "");
    }
}
