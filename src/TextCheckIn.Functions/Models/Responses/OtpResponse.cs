using System;

namespace TextCheckIn.Functions.Models.Responses
{
    /// <summary>
    /// Response model for OTP operations
    /// </summary>
    public class OtpResponse
    {

        /// <summary>
        /// Customer UUID (returned after successful verification)
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Masked phone number (returned after successful OTP send)
        /// </summary>
        public string? MaskedPhoneNumber { get; set; }

        /// <summary>
        /// Number of remaining OTP attempts
        /// </summary>
        public int? RemainingAttempts { get; set; }

        /// <summary>
        /// Indicates if this is a new customer
        /// </summary>
        public bool IsNewCustomer { get; set; }

        /// <summary>
        /// Remaining cooldown time in seconds before next OTP can be requested
        /// </summary>
        public int? CooldownSeconds { get; set; }
    }
}
