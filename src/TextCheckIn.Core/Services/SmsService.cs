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
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;

        private readonly SmsConfiguration _configuration;

        private readonly ILogger<SmsService> _logger;

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

        public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string firstName = "", string lastName = "")
        {
            var message = $"Your TextCheckIn verification code is: {otpCode}. This code will expire in 5 minutes.";
            return await SendSmsAsync(phoneNumber, message, firstName, lastName);
        }
    }
}
