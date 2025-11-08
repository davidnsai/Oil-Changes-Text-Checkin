namespace TextCheckIn.Core.Models.Configuration
{
    /// <summary>
    /// SMS service configuration
    /// </summary>
    public class SmsConfiguration
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
        public const string SectionName = "SmsService";

        /// <summary>
        /// SMS service Base URL
        /// </summary>
        public required string BaseUrl { get; set; }

        /// <summary>
        /// X-Identity-Key header value for authentication
        /// </summary>
        public required string IdentityKey { get; set; }

        /// <summary>
        /// X-functions-Key header value
        /// </summary>
        public required string FunctionsKey { get; set; }

    }
}
