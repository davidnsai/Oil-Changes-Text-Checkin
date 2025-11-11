using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Functions.Extensions;

public class OmniXHealthCheck : IHealthCheck
{
    private readonly OmniXConfiguration _config;
    private readonly IOmniXService _omniXService;

    public OmniXHealthCheck(
        IOptions<OmniXConfiguration> config,
        IOmniXService omniXService)
    {
        _config = config.Value;
        _omniXService = omniXService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>
            {
                ["ApiUrl"] = _config.ApiUrl ?? "not configured",
                ["HasApiKey"] = !string.IsNullOrEmpty(_config.ApiKey),
                ["HasWebhookSecret"] = !string.IsNullOrEmpty(_config.WebhookSecret),
                ["SignatureValidationEnabled"] = _config.EnableSignatureValidation,
                ["TimeoutSeconds"] = _config.TimeoutSeconds
            };

            if (string.IsNullOrEmpty(_config.ApiUrl))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("omniX API URL not configured", data: healthData));
            }

            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("omniX API key not configured", data: healthData));
            }

            if (string.IsNullOrEmpty(_config.WebhookSecret))
            {
                healthData["WebhookStatus"] = "webhook secret not configured";
            }

            return Task.FromResult(HealthCheckResult.Healthy("omniX service configuration is valid", healthData));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("omniX health check failed", ex));
        }
    }
}
