namespace TextCheckIn.Core.Models.Configuration
{
    public class SmsConfiguration
    {
        public const string SectionName = "SmsService";

        public required string BaseUrl { get; set; }

        public required string IdentityKey { get; set; }

        public required string FunctionsKey { get; set; }

    }
}
