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
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Functions.Functions
{
    public class CheckInFunction
    {
        private readonly ILogger<CheckInFunction> _logger;
        private readonly ICheckInSessionService _checkInSessionService;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ICheckInRepository _checkInRepository;
        private readonly ICustomersVehicleRepository _customersVehicleRepository;

        public CheckInFunction(
            ILogger<CheckInFunction> logger,
            ICheckInSessionService checkInSessionService,
            ISessionManagementService sessionManagementService,
            ICheckInRepository checkInRepository,
            ICustomersVehicleRepository customersVehicleRepository)
        {
            _logger = logger;
            _checkInSessionService = checkInSessionService;
            _sessionManagementService = sessionManagementService;
            _checkInRepository = checkInRepository;
            _customersVehicleRepository = customersVehicleRepository;
        }

        private async Task<HttpResponseData> CreateErrorResponseAsync<T>(
            HttpRequestData request,
            HttpStatusCode statusCode,
            string errorMessage,
            string requestId)
        {
            var response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Error = errorMessage,
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });
            return response;
        }

        [Function("GetRecentCheckIns")]
        public async Task<HttpResponseData> GetRecentCheckInsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "checkin/recent")]
            HttpRequestData request)
        {
            var requestId = Guid.NewGuid().ToString()[..8];
            _logger.LogDebug("GetRecentCheckIns {RequestId}: Received request", requestId);

            try
            {
                var locationParam = HttpUtility.ParseQueryString(request.Url.Query)["location"];

                if (string.IsNullOrWhiteSpace(locationParam))
                {
                    return await CreateErrorResponseAsync<List<VehicleResponse>>(request, HttpStatusCode.BadRequest, "The store location is required", requestId);
                }

                if (!Guid.TryParse(locationParam, out var locationId))
                {
                    return await CreateErrorResponseAsync<List<VehicleResponse>>(request, HttpStatusCode.BadRequest, "Store location format is invalid", requestId);
                }
                
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

                _logger.LogInformation("GetRecentCheckIns {RequestId}: Successfully returned {Count} vehicles for location {LocationId}",
                    requestId, vehicles.Count, locationId);

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<List<VehicleResponse>>
                {
                    Success = true,
                    Data = vehicles,
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString()
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRecentCheckIns {RequestId}: Error processing request", requestId);
                return await CreateErrorResponseAsync<List<VehicleResponse>>(request, HttpStatusCode.InternalServerError, "An error occurred while processing the request", requestId);
            }
        }

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

                if (checkInRequest == null || string.IsNullOrWhiteSpace(checkInRequest.PhoneNumber) || string.IsNullOrWhiteSpace(checkInRequest.StoreId))
                {
                    return await CreateErrorResponseAsync<object>(request, HttpStatusCode.BadRequest, "Phone number and store ID are required", requestId);
                }

                var session = await _checkInSessionService.LoginAsync(checkInRequest.PhoneNumber, checkInRequest.StoreId);

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = true,
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
                return await CreateErrorResponseAsync<object>(request, HttpStatusCode.InternalServerError, "An error occurred while processing the check-in", requestId);
            }
        }

        [Function("UpdateMileage")]
        public async Task<HttpResponseData> UpdateMileageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "checkin/{checkInId}/mileage")]
            HttpRequestData request,
            string checkInId)
        {
            var requestId = Guid.NewGuid().ToString()[..8];
            _logger.LogDebug("UpdateMileage {RequestId}: Received request for checkInId: {CheckInId}", requestId, checkInId);

            try
            {
                if (!Guid.TryParse(checkInId, out var checkInUuid))
                {
                    return await CreateErrorResponseAsync<bool>(request, HttpStatusCode.BadRequest, "Invalid check-in ID format", requestId);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var updateRequest = await JsonSerializer.DeserializeAsync<UpdateMileageRequest>(request.Body, options);

                if (updateRequest == null || updateRequest.Mileage < 0)
                {
                    return await CreateErrorResponseAsync<bool>(request, HttpStatusCode.BadRequest, "Invalid mileage value", requestId);
                }

                var checkIn = await _checkInRepository.GetCheckInByUuidAsync(checkInUuid);
                if (checkIn == null)
                {
                    return await CreateErrorResponseAsync<bool>(request, HttpStatusCode.NotFound, "Check-in not found", requestId);
                }

                checkIn.ActualMileage = updateRequest.Mileage;
                checkIn.CustomerId = _sessionManagementService.CurrentSession?.CustomerId;
                await _checkInRepository.UpdateCheckInAsync(checkIn);

                _logger.LogInformation("UpdateMileage {RequestId}: Successfully updated mileage to {Mileage} for check-in {CheckInId}",
                    requestId, updateRequest.Mileage, checkInId);

                // Attach vehicle to customer if not already attached
                if (checkIn.CustomerId.HasValue && checkIn.VehicleId.HasValue)
                {
                    var isAttached = await _customersVehicleRepository.ExistsAsync(checkIn.CustomerId.Value, checkIn.VehicleId.Value);
                    if (!isAttached)
                    {
                        var customersVehicle = new CustomersVehicle
                        {
                            CustomerId = checkIn.CustomerId.Value,
                            VehicleId = checkIn.VehicleId.Value
                        };
                        await _customersVehicleRepository.CreateAsync(customersVehicle);
                        _logger.LogInformation("UpdateMileage {RequestId}: Attached vehicle {VehicleId} to customer {CustomerId} for check-in {CheckInId}",
                            requestId, checkIn.VehicleId.Value, checkIn.CustomerId.Value, checkInId);
                    }
                }

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = true,
                    Data = null,
                    Message = "Mileage updated successfully",
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "UpdateMileage {RequestId}: Invalid JSON format", requestId);
                return await CreateErrorResponseAsync<bool>(request, HttpStatusCode.BadRequest, "Invalid request format", requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateMileage {RequestId}: Error updating mileage for check-in {CheckInId}", requestId, checkInId);
                return await CreateErrorResponseAsync<bool>(request, HttpStatusCode.InternalServerError, "An error occurred while updating the mileage", requestId);
            }
        }
    }
}
