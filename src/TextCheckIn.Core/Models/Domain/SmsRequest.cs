namespace TextCheckIn.Core.Models.Domain
{
    /// <summary>
    /// Represents an SMS request to the SMS service API
    /// </summary>
    public class SmsRequest
    {
        /// <summary>
        /// The communication channel - always "sms" for SMS messages
        /// </summary>
        public string Channel { get; set; } = "sms";

        /// <summary>
        /// The recipient phone number
        /// </summary>
        public required string Recipient { get; set; }

        /// <summary>
        /// The phone number (same as recipient)
        /// </summary>
        public required string Phone { get; set; }

        /// <summary>
        /// First name of the recipient
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Last name of the recipient
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// The SMS message content
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Subject line (may be ignored for SMS)
        /// </summary>
        public required string Subject { get; set; }
    }
}
