namespace TextCheckIn.Core.Models.Domain
{
    public class SmsRequest
    {
        public string Channel { get; set; } = "sms";

        public required string Recipient { get; set; }

        public required string Phone { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Content { get; set; }

        public required string Subject { get; set; }
    }
}
