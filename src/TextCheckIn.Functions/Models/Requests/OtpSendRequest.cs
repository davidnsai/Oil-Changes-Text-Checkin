using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TextCheckIn.Functions.Models.Requests
{
    public class OtpSendRequest
    {
        [Required]
        [Phone]
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }

        [JsonPropertyName("checkInId")]
        public required string CheckInId { get; set; }

        [JsonPropertyName("customerId")]
        public required string CustomerId { get; set; }
    }
}
