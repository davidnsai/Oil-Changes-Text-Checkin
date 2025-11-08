using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Functions.Extensions;

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
                ["ApiUrl"] = _config.ApiUrl ?? "not configured",
                ["HasApiKey"] = !string.IsNullOrEmpty(_config.ApiKey),
                ["HasWebhookSecret"] = !string.IsNullOrEmpty(_config.WebhookSecret),
                ["SignatureValidationEnabled"] = _config.EnableSignatureValidation,
                ["TimeoutSeconds"] = _config.TimeoutSeconds
            };

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

            return HealthCheckResult.Healthy("omniX service configuration is valid", healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("omniX health check failed", ex);
        }
    }
}
