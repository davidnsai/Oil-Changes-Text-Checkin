using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Functions.Extensions;

/// <summary>
/// Health check for omniX integration status
/// </summary>
public class OmniXHealthCheck : IHealthCheck
{
    private readonly OmniXConfiguration _config;
    private readonly OmniXServiceBase _omniXService;

    public OmniXHealthCheck(
        IOptions<OmniXConfiguration> config,
        OmniXServiceBase omniXServiceBase)
    {
        _config = config.Value;
        _omniXService = omniXServiceBase;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>
            {
                ["UseMockService"] = _config.UseMockService,
                ["ApiUrl"] = _config.ApiUrl ?? "not configured",
                ["HasApiKey"] = !string.IsNullOrEmpty(_config.ApiKey),
                ["HasWebhookSecret"] = !string.IsNullOrEmpty(_config.WebhookSecret),
                ["SignatureValidationEnabled"] = _config.EnableSignatureValidation,
                ["TimeoutSeconds"] = _config.TimeoutSeconds
            };

            if (_config.UseMockService)
            {
                // For mock service, just verify it can handle a basic operation
                try
                {
                    var request = new GetServiceRecommendationsByLicensePlateRequest
                    {
                        CheckInId = Guid.NewGuid(),
                        LicensePlate = "303",
                        StateCode = "CA",
                        Mileage = 202,
                        ClientLocationId = "200"
                    };

                    // Test mock service with a simple call
                    await _omniXService.GetServiceRecommendationAsync(request);
                    
                    healthData["MockServiceStatus"] = "operational";
                    return HealthCheckResult.Healthy("omniX mock service is operational", healthData);
                }
                catch (Exception ex)
                {
                    healthData["MockServiceError"] = ex.Message;
                    return HealthCheckResult.Degraded("omniX mock service has issues", ex, healthData);
                }
            }
            else
            {
                // For real service, check configuration and connectivity
                if (string.IsNullOrEmpty(_config.ApiUrl))
                {
                    return HealthCheckResult.Unhealthy("omniX API URL not configured", data: healthData);
                }

                if (string.IsNullOrEmpty(_config.ApiKey))
                {
                    return HealthCheckResult.Unhealthy("omniX API key not configured", data: healthData);
                }

                if (string.IsNullOrEmpty(_config.WebhookSecret))
                {
                    healthData["WebhookStatus"] = "webhook secret not configured";
                }

                // TODO: When real omniX service is implemented, add actual connectivity check
                // For now, just verify configuration is complete
                return HealthCheckResult.Healthy("omniX service configuration is valid", healthData);
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("omniX health check failed", ex);
        }
    }
}
