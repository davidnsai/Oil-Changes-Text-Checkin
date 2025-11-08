using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Models.Domain;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Core.Services
{
    /// <summary>
    /// Service for sending SMS messages via external SMS API
    /// </summary>
    public class SmsService : ISmsService
    {
        /// <summary>
        /// The HTTP client for making API requests
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Configuration settings for the SMS service
        /// </summary>
        private readonly SmsConfiguration _configuration;

        /// <summary>
        /// Logger for the SMS service
        /// </summary>
        private readonly ILogger<SmsService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsService"/> class
        /// </summary>
        /// <param name="httpClient">The HTTP client for making API requests</param>
        /// <param name="configuration">Configuration settings for the SMS service</param>
        /// <param name="logger">Logger for the SMS service</param>
        public SmsService(
            HttpClient httpClient,
            IOptions<SmsConfiguration> configuration,
            ILogger<SmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
            _logger = logger;

            // Configure HTTP client with base URL and headers
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Identity-Key", _configuration.IdentityKey);
            _httpClient.DefaultRequestHeaders.Add("x-functions-key", _configuration.FunctionsKey);
        }

        /// <inheritdoc/>
        /// <param name="phoneNumber">The recipient's phone number</param>
        /// <param name="message">The message content to send</param>
        /// <param name="firstName">The recipient's first name (defaults to empty string)</param>
        /// <param name="lastName">The recipient's last name (defaults to empty string)</param>
        /// <returns>True if the SMS was sent successfully, otherwise false</returns>
        public async Task<bool> SendSmsAsync(string phoneNumber, string message, string firstName = "", string lastName = "")
        {
            try
            {
                var smsRequest = new SmsRequest
                {
                    Channel = "sms",
                    Recipient = phoneNumber,
                    Phone = phoneNumber,
                    FirstName = string.IsNullOrEmpty(firstName) ? "Customer" : firstName,
                    LastName = string.IsNullOrEmpty(lastName) ? "" : lastName,
                    Content = message,
                    Subject = "TextCheckIn Notification"
                };

                var response = await _httpClient.PostAsJsonAsync("Alert", smsRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                    return true;
                }

                _logger.LogWarning(
                    "Failed to send SMS to {PhoneNumber}. Status: {StatusCode}, Reason: {Reason}",
                    phoneNumber,
                    response.StatusCode,
                    response.ReasonPhrase);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        /// <inheritdoc/>
        /// <param name="phoneNumber">The recipient's phone number</param>
        /// <param name="otpCode">The one-time password code to send</param>
        /// <param name="firstName">The recipient's first name (defaults to empty string)</param>
        /// <param name="lastName">The recipient's last name (defaults to empty string)</param>
        /// <returns>True if the OTP SMS was sent successfully, otherwise false</returns>
        public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string firstName = "", string lastName = "")
        {
            var message = $"Your TextCheckIn verification code is: {otpCode}. This code will expire in 5 minutes.";
            return await SendSmsAsync(phoneNumber, message, firstName, lastName);
        }
    }
}
