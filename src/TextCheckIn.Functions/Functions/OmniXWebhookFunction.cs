using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TextCheckIn.Core.Helpers;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Functions.Models.Responses;

namespace TextCheckIn.Functions.Functions;

public class OmniXWebhookFunction
{
    private readonly IOmniXService _omniXService;
    private readonly OmniXWebhookSignatureValidator _webhookSignatureValidator;
    private readonly ILogger<OmniXWebhookFunction> _logger;

    public OmniXWebhookFunction(
        IOmniXService omniXService,
        OmniXWebhookSignatureValidator webhookSignatureValidator,
        ILogger<OmniXWebhookFunction> logger)
    {
        _omniXService = omniXService;
        _webhookSignatureValidator = webhookSignatureValidator;
        _logger = logger;
    }

    [Function("OmniXWebhook")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook/omnix")]
        HttpRequestData req,
        FunctionContext context)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        _logger.LogInformation("Webhook {RequestId}: Received omniX notification", requestId);

        try
        {
            // Step 1: Fast validation and authentication
            var validationResult = await ValidateWebhookRequestAsync(req, requestId);
            if (!validationResult.IsValid)
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.Unauthorized,
                    validationResult.ErrorMessage!, requestId);
            }

            // Step 2: Parse and validate payload
            var serviceRecommendation = await ParseServiceRecommendationPayloadAsync(validationResult.Payload!, requestId);
            if (serviceRecommendation == null)
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest,
                    "Invalid notification payload format", requestId);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);

            // Process the service recommendation
            try
            {
                await _omniXService.ProcessIncomingServiceRecommendationAsync(serviceRecommendation);
                _logger.LogInformation("Webhook {RequestId}: Processing completed for {LicensePlate}",
                    requestId, serviceRecommendation.LicensePlate);
                await response.WriteAsJsonAsync(new WebhookResponse
                {
                    Success = true,
                    Message = "Notification processed successfully",
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook {RequestId}: Error during processing for {LicensePlate}",
                    requestId, serviceRecommendation.LicensePlate);

                await response.WriteAsJsonAsync(new WebhookResponse
                {
                    Success = false,
                    Message = $"Error processing notification: {ex.Message}",
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook {RequestId}: Unexpected error during processing", requestId);
            return await CreateErrorResponseAsync(req, HttpStatusCode.InternalServerError,
                "Internal server error", requestId);
        }
    }

    private async Task<WebhookValidationResult> ValidateWebhookRequestAsync(
        HttpRequestData req,
        string requestId)
    {
        try
        {
            // Check Content-Type
            var contentType = req.Headers.GetValues("Content-Type").FirstOrDefault();
            if (contentType != "application/json")
            {
                _logger.LogWarning("Webhook {RequestId}: Invalid Content-Type: {ContentType}",
                    requestId, contentType);
                return WebhookValidationResult.Invalid("Invalid Content-Type. Expected application/json");
            }

            // Read payload
            var payload = await ReadRequestBodyAsync(req);
            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Webhook {RequestId}: Empty payload received", requestId);
                return WebhookValidationResult.Invalid("Empty payload");
            }

            // Check payload size
            if (payload.Length > 1048576) // 1MB limit
            {
                _logger.LogWarning("Webhook {RequestId}: Payload too large: {Size} bytes",
                    requestId, payload.Length);
                return WebhookValidationResult.Invalid("Payload too large");
            }

            // Get signature and timestamp headers
            var signature = req.Headers.GetValues("X-OmniX-Signature").FirstOrDefault();
            var timestamp = req.Headers.GetValues("X-OmniX-Timestamp").FirstOrDefault();

            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
            {
                _logger.LogWarning("Webhook {RequestId}: Missing required headers", requestId);
                return WebhookValidationResult.Invalid("Missing signature or timestamp headers");
            }

            // Validate signature
            if (!_webhookSignatureValidator.ValidateWebhookSignature(payload, signature, timestamp))
            {
                _logger.LogWarning("Webhook {RequestId}: Invalid signature", requestId);
                return WebhookValidationResult.Invalid("Invalid webhook signature");
            }

            _logger.LogDebug("Webhook {RequestId}: Validation successful", requestId);
            return WebhookValidationResult.Valid(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook {RequestId}: Error during validation", requestId);
            return WebhookValidationResult.Invalid("Validation error");
        }
    }

    private Task<ServiceRecommendation?> ParseServiceRecommendationPayloadAsync(string payload, string requestId)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var serviceRecommendation = JsonSerializer.Deserialize<ServiceRecommendation>(payload, options);

            if (serviceRecommendation == null)
            {
                _logger.LogWarning("Webhook {RequestId}: Failed to deserialize notification", requestId);
                return Task.FromResult<ServiceRecommendation?>(null);
            }

            // Basic validation of required fields
            if (string.IsNullOrEmpty(serviceRecommendation.LicensePlate) ||
                string.IsNullOrEmpty(serviceRecommendation.StateCode) ||
                string.IsNullOrEmpty(serviceRecommendation.ClientLocationId))
            {
                _logger.LogWarning("Webhook {RequestId}: Missing required fields in notification", requestId);
                return Task.FromResult<ServiceRecommendation?>(null);
            }

            _logger.LogDebug("Webhook {RequestId}: Parsed notification for {LicensePlate}",
                requestId, serviceRecommendation.LicensePlate);

            return Task.FromResult<ServiceRecommendation?>(serviceRecommendation);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Webhook {RequestId}: JSON parsing error", requestId);
            return Task.FromResult<ServiceRecommendation?>(null);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequestData req)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static async Task<HttpResponseData> CreateErrorResponseAsync(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string message,
        string requestId)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new WebhookResponse
        {
            Success = false,
            Message = message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        });
        return response;
    }
}

internal class WebhookValidationResult
{
    public bool IsValid { get; private set; }
    public string? Payload { get; private set; }
    public string? ErrorMessage { get; private set; }

    private WebhookValidationResult(bool isValid, string? payload, string? errorMessage)
    {
        IsValid = isValid;
        Payload = payload;
        ErrorMessage = errorMessage;
    }

    public static WebhookValidationResult Valid(string payload) =>
        new(true, payload, null);

    public static WebhookValidationResult Invalid(string errorMessage) =>
        new(false, null, errorMessage);
}
