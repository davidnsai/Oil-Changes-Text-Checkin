using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Requests
{
    /// <summary>
    /// Request model for sending OTP
    /// </summary>
    public class OtpSendRequest
    {
        /// <summary>
        /// Phone number to send OTP to
        /// </summary>
        [Required]
        [Phone]
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Check in Id for the OTP request
        /// </summary>
        /// [Required]
        [JsonPropertyName("checkInId")]
        public required string CheckInId { get; set; }

        /// <summary>
        /// Optional customer ID for existing customers
        /// </summary>
        [JsonPropertyName("customerId")]
        public required string CustomerId { get; set; }
    }
}
