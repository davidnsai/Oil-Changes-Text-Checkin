using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TextCheckIn.Functions.Functions
{
    public class VideoSasTokenFunction
    {
        private readonly ILogger<VideoSasTokenFunction> _logger;

        // Allowlist of video filenames that can receive SAS tokens
        private static readonly HashSet<string> AllowedVideoFiles = new(StringComparer.OrdinalIgnoreCase)
        {
            // Power Services
            "transmission-service.mp4",
            "coolant-flush.mp4",
            "intake-cleaning.mp4",
            "power-steering.mp4",
            "gear-box-service.mp4",
            "cabin-air-filter.mp4",

            // Legacy/other
            "oil-change.mp4",
            "tire-rotation.mp4",
            "differential-service.mp4",
            "synthetic-oil.mp4",
            "high-mileage-oil.mp4",
            "conventional-oil.mp4",
        };

        private static readonly Regex AllowedExtension = new(@"\.(mp4|webm|mov|m4v)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public VideoSasTokenFunction(ILogger<VideoSasTokenFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetVideoSasUrl")]
        public async Task<HttpResponseData> GetVideoSasUrlAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "videos/sas")]
            HttpRequestData req)
        {
            var requestId = Guid.NewGuid().ToString()[..8];
            _logger.LogInformation("VideoSasToken {RequestId}: Token request received", requestId);

            // Parse the query string for fileName parameter
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var fileName = queryParams["fileName"];

            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("VideoSasToken {RequestId}: Missing fileName parameter", requestId);

                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Missing required parameter: fileName" });
                return badRequestResponse;
            }
            try
            {
                // Validate filename against allowlist and extension
                fileName = fileName.Trim();
                if (!AllowedExtension.IsMatch(fileName) || !AllowedVideoFiles.Contains(fileName))
                {
                    _logger.LogWarning("VideoSasToken {RequestId}: File not allowed: {FileName}", requestId, fileName);
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteAsJsonAsync(new { error = "File not allowed" });
                    return forbidden;
                }
                
                // Get storage configuration
                string connectionString = Environment.GetEnvironmentVariable("AzureStorage") ?? string.Empty;
                string containerName = Environment.GetEnvironmentVariable("VideoContainer") ?? "text-check-in-videos";

                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("VideoSasToken {RequestId}: Storage connection string not configured", requestId);

                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteAsJsonAsync(new { error = "Storage configuration error" });
                    return errorResponse;
                }

                // Create blob clients
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                // Check if the blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    _logger.LogWarning("VideoSasToken {RequestId}: Video file not found: {FileName}", requestId, fileName);

                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new { error = "Video file not found" });
                    return notFoundResponse;
                }

                // Generate a SAS token with 15 minute expiration (read-only)
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = fileName,
                    Resource = "b", // "b" for blob
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15) // Token valid for 15 minutes
                };

                // Set read-only permission
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Generate the SAS URI
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                // Return the full URL with the SAS token
                var response = req.CreateResponse(HttpStatusCode.OK);

                // CORS: allow specific origins only (from env comma-separated)
                // e.g. AllowedOrigins="http://localhost:3000,https://your-prod-domain"
                var allowedOriginsEnv = Environment.GetEnvironmentVariable("AllowedOrigins")
                    ?? Environment.GetEnvironmentVariable("CORSOrigins");
                if (!string.IsNullOrWhiteSpace(allowedOriginsEnv))
                {
                    var allowedOrigins = new HashSet<string>(allowedOriginsEnv
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);

                    if (req.Headers.TryGetValues("Origin", out var originValues))
                    {
                        var origin = originValues.FirstOrDefault();
                        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
                        {
                            response.Headers.Add("Access-Control-Allow-Origin", origin);
                            response.Headers.Add("Vary", "Origin");
                        }
                    }
                }

                // No-cache headers to prevent storing SAS responses
                response.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0, must-revalidate");
                response.Headers.Add("Pragma", "no-cache");

                await response.WriteAsJsonAsync(new
                {
                    videoUrl = sasUri.ToString(),
                    expiresAt = sasBuilder.ExpiresOn
                });

                _logger.LogInformation("VideoSasToken {RequestId}: Token generated for {FileName}, expires at {ExpiryTime}",
                    requestId, fileName, sasBuilder.ExpiresOn);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VideoSasToken {RequestId}: Error generating token for {FileName}: {ErrorMessage}",
                    requestId, fileName, ex.Message);

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = "Failed to generate video access token" });
                return errorResponse;
            }
        }
    }
}
