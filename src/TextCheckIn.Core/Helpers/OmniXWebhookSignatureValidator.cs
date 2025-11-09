using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using TextCheckIn.Core.Models.Configuration;

namespace TextCheckIn.Core.Helpers;

public class OmniXWebhookSignatureValidator
{
    private readonly OmniXConfiguration _config;
    private readonly ILogger<OmniXWebhookSignatureValidator> _logger;

    public OmniXWebhookSignatureValidator(IOptions<OmniXConfiguration> options, ILogger<OmniXWebhookSignatureValidator> logger)
    {
        _config = options.Value;
        _logger = logger;
    }

    public bool ValidateWebhookSignature(string payload, string signature, string timestamp)
    {
        if (!_config.EnableSignatureValidation)
        {
            _logger.LogDebug("Signature validation disabled");
            return true;
        }

        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            _logger.LogWarning("No webhook secret configured");
            return false;
        }

        if (!DateTime.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var webhookTime))
        {
            _logger.LogWarning("Invalid timestamp format: {Timestamp}", timestamp);
            return false;
        }

        var age = DateTime.UtcNow - webhookTime;
        if (Math.Abs(age.TotalMinutes) > _config.MaxWebhookAgeMinutes)
        {
            _logger.LogWarning("Timestamp too old or too new: {Age} minutes", age.TotalMinutes);
            return false;
        }

        var signedPayload = $"{timestamp}.{payload}";
        var expectedSignature = ComputeHmacSha256(signedPayload, _config.WebhookSecret);

        var expectedSignatureBase64 = Convert.ToBase64String(expectedSignature);
        
        _logger.LogWarning("Validating webhook signature. Expected: {Expected}, Received: {Received}",
            expectedSignatureBase64, signature);

        byte[] receivedSignatureBytes;
        try
        {
            receivedSignatureBytes = Convert.FromBase64String(signature);
        }
        catch (FormatException)
        {
            _logger.LogWarning("Signature is not a valid Base64 string");
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedSignature, receivedSignatureBytes);
    }

    private static byte[] ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return hash;
    }
}

