using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Requests
{
    public class OtpVerifyRequest : BaseOtpRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [JsonPropertyName("otpCode")]
        public required string OtpCode { get; set; }
    }
}
