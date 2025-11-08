using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Requests
{
    /// <summary>
    /// Request model for verifying OTP
    /// </summary>
    public class OtpVerifyRequest
    {
        /// <summary>
        /// Phone number associated with the OTP
        /// </summary>
        [Required]
        [Phone]
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Customer ID associated with the OTP
        /// </summary>
        [Required]
        [JsonPropertyName("customerId")]
        public required string CustomerId { get; set; }

        /// <summary>
        /// Check in ID associated with the OTP
        /// </summary>
        [Required]
        [JsonPropertyName("checkInId")]
        public required string CheckInId { get; set; }

        /// <summary>
        /// OTP code to verify
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [JsonPropertyName("otpCode")]
        public required string OtpCode { get; set; }
    }
}
