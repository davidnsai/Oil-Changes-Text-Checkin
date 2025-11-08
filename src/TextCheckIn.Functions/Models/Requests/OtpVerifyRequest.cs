using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Requests
{
    public class OtpVerifyRequest
    {
        [Required]
        [Phone]
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [Required]
        [JsonPropertyName("customerId")]
        public required string CustomerId { get; set; }

        [Required]
        [JsonPropertyName("checkInId")]
        public required string CheckInId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [JsonPropertyName("otpCode")]
        public required string OtpCode { get; set; }
    }
}
