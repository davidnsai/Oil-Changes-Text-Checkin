using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Functions.Models.Responses;

namespace TextCheckIn.Functions.Middleware
{
    public class SessionMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<SessionMiddleware> _logger;

        public SessionMiddleware(ILogger<SessionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            _logger.LogInformation("Processing request in SessionMiddleware");

            var httpRequestData = await context.GetHttpRequestDataAsync();

            // Skip session validation for health, webhook, and checkin/recent routes
            if (httpRequestData != null &&
                (httpRequestData.Url.AbsolutePath.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
                 httpRequestData.Url.AbsolutePath.Contains("/webhook", StringComparison.OrdinalIgnoreCase) ||
                 httpRequestData.Url.AbsolutePath.Contains("/ping", StringComparison.OrdinalIgnoreCase))
            )
            {
                await next(context);
                return;
            }

            if (httpRequestData == null)
            {
                _logger.LogWarning("No HTTP request context found.");
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Invalid HTTP request context.");
                return;
            }

            try
            {
                var sessionService = context.InstanceServices.GetRequiredService<ISessionManagementService>();
                var ipAddress = GetClientIpAddress(httpRequestData);
                var sessionId = GetSessionId(httpRequestData);

                if (sessionId == null)
                {
                    _logger.LogWarning("Missing session ID in request headers.");
                    await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Missing session ID in request.");
                    return;
                }

                // Try to get existing session (do not create new ones)
                var existingSession = await sessionService.GetSessionAsync(sessionId.Value);

                if (existingSession == null)
                {
                    if(httpRequestData.Url.AbsolutePath.Contains("/checkin/recent", StringComparison.OrdinalIgnoreCase))
                    {
                        await sessionService.CreateNewSessionAsync();
                    }
                    else
                    {
                        _logger.LogWarning("Session {SessionId} not found or expired.", sessionId);
                        await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Session not found or expired. Please start a new session.");
                        return;
                    }
                }

                // Session is valid
                if (sessionService.CurrentSession != null)
                {
                    context.Items["SessionId"] = sessionService.CurrentSession.Id;
                }
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in session middleware");
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "Internal server error in session validation.");
            }
        }

        private static string? GetClientIpAddress(HttpRequestData request)
        {
            if (request.Headers.TryGetValues("X-Forwarded-For", out var forwardedIps))
            {
                var forwardedIp = forwardedIps.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(forwardedIp))
                    return forwardedIp;
            }

            if (request.Headers.TryGetValues("X-Real-IP", out var realIps))
            {
                var realIp = realIps.FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(realIp))
                    return realIp;
            }

            return null;
        }

        private static Guid? GetSessionId(HttpRequestData request)
        {
            if (request.Headers.TryGetValues("X-Session-Id", out var sessionIdValues))
            {
                var sessionIdStr = sessionIdValues.FirstOrDefault();
                if (Guid.TryParse(sessionIdStr, out var sessionId))
                    return sessionId;
            }
            return null;
        }

        private static async Task WriteErrorResponse(FunctionContext context, HttpStatusCode status, string message)
        {
            var request = await context.GetHttpRequestDataAsync();
            if (request == null) return;

            var response = request.CreateResponse(status);
            await response.WriteAsJsonAsync(new ApiResponse<object>
            {
                Data = null,
                Error = message,
                Timestamp = DateTime.UtcNow,
                RequestId = Guid.NewGuid().ToString(),
                SessionId = null
            });

            context.GetInvocationResult().Value = response;
        }
    }
}
