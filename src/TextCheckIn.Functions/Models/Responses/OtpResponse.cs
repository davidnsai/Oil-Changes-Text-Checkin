using System;

namespace TextCheckIn.Functions.Models.Responses
{
    public class OtpResponse
    {

        public Guid? CustomerId { get; set; }

        public string? MaskedPhoneNumber { get; set; }

        public int? RemainingAttempts { get; set; }

        public bool IsNewCustomer { get; set; }

        public int? CooldownSeconds { get; set; }
    }
}
