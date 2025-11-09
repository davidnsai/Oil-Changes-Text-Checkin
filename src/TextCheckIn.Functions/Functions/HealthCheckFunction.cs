using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using TextCheckIn.Functions.Models.Responses;

namespace TextCheckIn.Functions.Functions;

public class HealthCheckFunction
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckFunction> _logger;

    public HealthCheckFunction(
        HealthCheckService healthCheckService,
        ILogger<HealthCheckFunction> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    [Function("HealthCheck")]
    public async Task<HttpResponseData> HealthCheckAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] 
        HttpRequestData req)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        
        try
        {
            _logger.LogDebug("HealthCheck {RequestId}: Performing system health check", requestId);

            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var status = healthReport.Status switch
            {
                HealthStatus.Healthy => HttpStatusCode.OK,
                HealthStatus.Degraded => HttpStatusCode.OK, // Still OK but with warnings
                HealthStatus.Unhealthy => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.ServiceUnavailable
            };

            var response = req.CreateResponse(status);
            await response.WriteAsJsonAsync(new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Entries = healthReport.Entries.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        Status = kvp.Value.Status.ToString(),
                        Description = kvp.Value.Description,
                        Duration = kvp.Value.Duration.TotalMilliseconds,
                        Data = kvp.Value.Data,
                        Exception = kvp.Value.Exception?.Message
                    }),
                RequestId = requestId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("HealthCheck {RequestId}: System status is {Status}", 
                requestId, healthReport.Status);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthCheck {RequestId}: Health check failed", requestId);
            
            var response = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            await response.WriteAsJsonAsync(new ApiResponse<object>
            {
                Error = "Health check failed",
                RequestId = requestId,
                Timestamp = DateTime.UtcNow
            });
            
            return response;
        }
    }

    [Function("Ping")]
    public async Task<HttpResponseData> PingAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] 
        HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            Message = "Service is alive",
            Timestamp = DateTime.UtcNow,
            Version = typeof(HealthCheckFunction).Assembly.GetName().Version?.ToString() ?? "unknown"
        });

        return response;
    }
}
