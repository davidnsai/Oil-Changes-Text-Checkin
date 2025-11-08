using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TextCheckIn.Functions.Models.Responses;
using TextCheckIn.Functions.Models.Requests;
using TextCheckIn.Core.Services.Interfaces;
using System.Text.Json;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Functions.Functions
{
    public class CheckInFunction
    {
        private readonly ILogger<CheckInFunction> _logger;
        private readonly ICheckInSessionService _checkInSessionService;
        private readonly ISessionManagementService _sessionManagementService;

        public CheckInFunction(
            ILogger<CheckInFunction> logger,
            ICheckInSessionService checkInSessionService,
            ISessionManagementService sessionManagementService)
        {
            _logger = logger;
            _checkInSessionService = checkInSessionService;
            _sessionManagementService = sessionManagementService;
        }

        /// <summary>
        /// Endpoint to get recent check-ins
        /// GET /api/checkin/recent?location={locationId}
        /// </summary>
        [Function("GetRecentCheckIns")]
        public async Task<HttpResponseData> GetRecentCheckInsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "checkin/recent")]
        HttpRequestData request)
        {
            var requestId = Guid.NewGuid().ToString()[..8];

            _logger.LogDebug("GetRecentCheckIns {RequestId}: Received request", requestId);

            try
            {
                // Parse location query parameter
                var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                var locationParam = queryParams["location"];

                if (string.IsNullOrWhiteSpace(locationParam))
                {
                    _logger.LogWarning("GetRecentCheckIns {RequestId}: Missing location parameter", requestId);
                    
                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<List<string>>
                    {
                        Data = null,
                        Error = "The store location is required",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                // Validate location parameter as Guid
                if (!Guid.TryParse(locationParam, out var locationId))
                {
                    _logger.LogWarning("GetRecentCheckIns {RequestId}: Invalid location parameter format: {LocationParam}", 
                        requestId, locationParam);
                    
                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<List<string>>
                    {
                        Data = null,
                        Error = "Store location format is invalid",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                _logger.LogInformation("GetRecentCheckIns {RequestId}: Processing request for location: {LocationId}", 
                    requestId, locationId);

                // Get recent check-ins for the location
                var checkings = await _checkInSessionService.GetRecentCheckInsByLocationAsync(locationId);

                var vehicles = checkings.Select(c => new VehicleResponse
                {
                    Id = c.Vehicle!.VehicleUUID,
                    CheckInId = c.Uuid, 
                    Vin = c.Vehicle!.Vin,
                    LicensePlate = c.Vehicle!.LicensePlate,
                    StateCode = c.Vehicle!.StateCode,
                    Make = c.Vehicle!.Make,
                    Model = c.Vehicle!.Model,
                    YearOfMake = c.Vehicle!.YearOfMake,
                    LastMileage = c.Vehicle!.LastMileage
                }).ToList();

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<List<VehicleResponse>>
                {
                    Success = true,
                    Data = vehicles,
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString()
                });

                _logger.LogInformation("GetRecentCheckIns {RequestId}: Successfully returned {Count} license plates", 
                    requestId, vehicles.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRecentCheckIns {RequestId}: Error processing request", requestId);
                
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<List<string>>
                {
                    Data = null,
                    Error = "An error occurred while processing the request",
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });
                return errorResponse;
            }
        }

        /// <summary>
        /// Endpoint to perform a check-in
        /// POST /api/checkin/submit
        /// </summary>
        [Function("PerformCheckIn")]
        public async Task<HttpResponseData> PerformCheckInAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkin/submit")]
        HttpRequestData request)
        {
            var requestId = Guid.NewGuid().ToString()[..8];

            _logger.LogDebug("PerformCheckIn {RequestId}: Received request", requestId);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var checkInRequest = JsonSerializer.Deserialize<CheckInRequest>(request.Body, options);


                // Validate the request
                if (checkInRequest == null || string.IsNullOrWhiteSpace(checkInRequest.PhoneNumber) || string.IsNullOrWhiteSpace(checkInRequest.StoreId))
                {
                    var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "Phone number and store ID are required",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                // Perform login/check-in using the service
                var session = await _checkInSessionService.LoginAsync(
                    checkInRequest.PhoneNumber, 
                    checkInRequest.StoreId);

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Data = session,
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PerformCheckIn {RequestId}: Error processing check-in", requestId);
                
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Data = null,
                    Error = "An error occurred while processing the check-in",
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });
                return errorResponse;
            }
        }
    }
}
