using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using TextCheckIn.Core.Models.Configuration;

namespace TextCheckIn.Core.Helpers;

/// <summary>
/// Validates signatures of OmniX webhooks to ensure they originate from a trusted source.
/// </summary>
/// <remarks>
/// This validator helps prevent unauthorized webhook calls by verifying their cryptographic signatures
/// and protecting against replay attacks through timestamp validation.
/// </remarks>
public class OmniXWebhookSignatureValidator
{
    private readonly OmniXConfiguration _config;
    private readonly ILogger<OmniXWebhookSignatureValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmniXWebhookSignatureValidator"/> class.
    /// </summary>
    /// <param name="options">The OmniX configuration options.</param>
    /// <param name="logger">The logger used for diagnostic information.</param>
    public OmniXWebhookSignatureValidator(IOptions<OmniXConfiguration> options, ILogger<OmniXWebhookSignatureValidator> logger)
    {
        _config = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates the signature of an OmniX webhook request.
    /// </summary>
    /// <param name="payload">The webhook payload to validate.</param>
    /// <param name="signature">The signature provided with the webhook.</param>
    /// <param name="timestamp">The timestamp when the webhook was sent.</param>
    /// <returns>
    /// <c>true</c> if the signature is valid and the webhook passes all security checks;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs several security checks:
    /// 1. Verifies signature validation is enabled
    /// 2. Ensures a webhook secret is configured
    /// 3. Validates the timestamp format
    /// 4. Checks if the webhook is within the allowed time window
    /// 5. Computes and compares signatures using constant-time comparison
    /// </remarks>
    public bool ValidateWebhookSignature(string payload, string signature, string timestamp)
    {
        if (!_config.EnableSignatureValidation)
        {
            _logger.LogDebug("Mock webhook: Signature validation disabled");
            return true;
        }

        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            _logger.LogWarning("Mock webhook: No webhook secret configured");
            return false;
        }

        // Validate timestamp (replay attack prevention)
        if (!DateTime.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var webhookTime))
        {
            _logger.LogWarning("Mock webhook: Invalid timestamp format: {Timestamp}", timestamp);
            return false;
        }

        // Check for replay attack (timestamp too old or too new)
        var age = DateTime.UtcNow - webhookTime;
        if (Math.Abs(age.TotalMinutes) > _config.MaxWebhookAgeMinutes)
        {
            _logger.LogWarning("Mock webhook: Timestamp too old or too new: {Age} minutes", age.TotalMinutes);
            return false;
        }

        // Include timestamp in the signature to prevent replay attacks
        var signedPayload = $"{timestamp}.{payload}";
        var expectedSignature = ComputeHmacSha256(signedPayload, _config.WebhookSecret);

        var expectedSignatureBase64 = Convert.ToBase64String(expectedSignature);
        
        _logger.LogWarning("Validating webhook signature. Expected: {Expected}, Received: {Received}",
            expectedSignatureBase64, signature);

        byte[] receivedSignatureBytes;
        try
        {
            // Ensure the signature is a valid Base64 string
            receivedSignatureBytes = Convert.FromBase64String(signature);
        }
        catch (FormatException)
        {
            _logger.LogWarning("Mock webhook: Signature is not a valid Base64 string");
            return false;
        }
        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(expectedSignature, receivedSignatureBytes);
    }


    /// <summary>
    /// Computes an HMAC-SHA256 hash of the provided data using the specified secret.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The secret key to use for hashing.</param>
    /// <returns>A base64-encoded string representation of the computed hash.</returns>
    private static byte[] ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return hash;
    }
}

