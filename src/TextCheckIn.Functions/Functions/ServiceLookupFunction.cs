using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Functions.Models.Requests;
using TextCheckIn.Functions.Models.Responses;
using TextCheckIn.Shared.Models;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace TextCheckIn.Functions.Functions;

public class ServiceLookupFunction
{
    private readonly IOmniXService _omniXService;
    private readonly ISessionManagementService _sessionManagementService;
    private readonly ICheckInServiceRepository _checkInServiceRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger<ServiceLookupFunction> _logger;

    public ServiceLookupFunction(
        IOmniXService omniXService,
        ISessionManagementService sessionManagementService,
        ICheckInServiceRepository checkInServiceRepository,
        ICheckInRepository checkInRepository,
        ILogger<ServiceLookupFunction> logger)
    {
        _omniXService = omniXService;
        _sessionManagementService = sessionManagementService;
        _checkInServiceRepository = checkInServiceRepository;
        _checkInRepository = checkInRepository;
        _logger = logger;
    }

    [Function("GetServiceRecommendationsByCheckInUuid")]
    public async Task<HttpResponseData> GetServiceRecommendationsByCheckInUuidAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "services/recommended/get-by-check-in-uuid")] 
        HttpRequestData request)
    {
        var requestId = Guid.NewGuid().ToString()[..8];

        try
        {
            var requestData = await request.ReadFromJsonAsync<GetServiceRecommendationsByCheckInUuidRequest>();
            _logger.LogInformation($"GetServiceRecommendationsByCheckInUuid {requestId}: " +
                $"Getting recommendations for check-in {requestData!.CheckInUuid}");

            var validationContext = new ValidationContext(requestData);
            var validationResults = new List<ValidationResult>();

            if(!Validator.TryValidateObject(requestData, validationContext, validationResults, true))
            {
                var message = "The request body has one or more invalid fields.";
                return await CreateErrorResponseAsync(request, HttpStatusCode.BadRequest, message, requestId, validationResults);
            }

            var serviceRecommendations = await _omniXService.GetServiceRecommendationAsync(requestData);

            if (serviceRecommendations == null || !serviceRecommendations.Any())
            {
                var message = $"No service recommendations found for check-in {requestData.CheckInUuid}.";
                return await CreateErrorResponseAsync(request, HttpStatusCode.NotFound, message, requestId);
            }

            _logger.LogInformation($"GetServiceRecommendationsByCheckInUuid {requestId}: " +
                $"Found {serviceRecommendations.Count()} recommendations for check-in {requestData.CheckInUuid}");

            var response = request.CreateResponse(HttpStatusCode.OK);

            // Map entities to DTOs to avoid circular reference issues
            var serviceRecommendationDtos = serviceRecommendations.Select(cis => new ServiceRecommendationResponse
            {
                ServiceUuid = cis.Service.ServiceUuid,
                ServiceName = cis.Service.Name,
                ServiceDescription = cis.Service.Description,
                Price = cis.Service.Price,
                EstimatedDurationMinutes = cis.Service.EstimatedDurationMinutes,
                IsCustomerSelected = cis.IsCustomerSelected,
                IntervalMiles = cis.IntervalMiles,
                LastServiceMiles = cis.LastServiceMiles,
                MileageBucket = cis.MileageBucket
            }).ToList();

            await response.WriteAsJsonAsync(new ApiResponse<List<ServiceRecommendationResponse>>() 
            { 
                Success = true,
                Data = serviceRecommendationDtos,
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });

            return response;
        }
        catch(AggregateException)
        {
            var message = "The request body is missing one or more required fields.";
            return await CreateErrorResponseAsync(request, HttpStatusCode.BadRequest, message, requestId);
        }
        catch (Exception ex)
        {
            var errorMessage = "Error retrieving service recommendation";
            _logger.LogError(ex, $"{errorMessage}.\nRequestId: {requestId}.\nDetail: {ex.Message}");

            return await CreateErrorResponseAsync(request, HttpStatusCode.InternalServerError, errorMessage, requestId);
        }
    }

    [Function("GetPowerServices")]
    public async Task<HttpResponseData> GetPowerServicesAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "services/power")] 
        HttpRequestData req)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        
        try
        {
            _logger.LogInformation("ServiceCatalog {RequestId}: Getting Power-6 service catalog", requestId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    Services = Power6ServiceCatalog.Services.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description,
                        s.Price,
                        s.EstimatedDurationMinutes,
                        s.Category,
                        s.IsPopular
                    }).ToList(),
                    TotalServices = Power6ServiceCatalog.Services.Count
                },
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ServiceCatalog {RequestId}: Error getting Power-6 services", requestId);
            
            return await CreateErrorResponseAsync(req, HttpStatusCode.InternalServerError, 
                "Internal server error", requestId);
        }
    }

    [Function("UpdateServiceRecommendations")]
    public async Task<HttpResponseData> UpdateServiceRecommendationsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "services/recommended")] 
        HttpRequestData request)
    {
        var requestId = Guid.NewGuid().ToString()[..8];

        try
        {
            var requestData = await request.ReadFromJsonAsync<UpdateServiceRecommendationsRequest>();
            _logger.LogInformation($"UpdateServiceRecommendations {requestId}: " +
                $"Updating service recommendations for check-in {requestData!.CheckInUuid}");

            var validationContext = new ValidationContext(requestData);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(requestData, validationContext, validationResults, true))
            {
                var message = "The request body has one or more invalid fields.";
                return await CreateErrorResponseAsync(request, HttpStatusCode.BadRequest, message, requestId, validationResults);
            }

            // Verify that the check-in exists
            var checkIn = await _checkInRepository.GetCheckInByUuidAsync(requestData.CheckInUuid);
            if (checkIn == null)
            {
                var message = $"Check-in with UUID {requestData.CheckInUuid} not found.";
                return await CreateErrorResponseAsync(request, HttpStatusCode.NotFound, message, requestId);
            }

            // Convert the service selections to a dictionary
            var serviceSelections = requestData.Services.ToDictionary(s => s.ServiceUuid, s => s.IsCustomerSelected);

            // Update the service recommendations
            var updateResult = await _checkInServiceRepository.UpdateCheckInServicesAsync(
                requestData.CheckInUuid, serviceSelections);

            if (!updateResult)
            {
                var message = "Failed to update service recommendations. No matching services found for the provided service IDs.";
                return await CreateErrorResponseAsync(request, HttpStatusCode.NotFound, message, requestId);
            }

            _logger.LogInformation($"UpdateServiceRecommendations {requestId}: " +
                $"Successfully updated {requestData.Services.Count} service recommendations for check-in {requestData.CheckInUuid}");

            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = true,
                Message = "Service recommendations updated successfully",
                Timestamp = DateTime.UtcNow,
                RequestId = requestId,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });

            return response;
        }
        catch (AggregateException)
        {
            var message = "The request body is missing one or more required fields.";
            return await CreateErrorResponseAsync(request, HttpStatusCode.BadRequest, message, requestId);
        }
        catch (Exception ex)
        {
            var errorMessage = "Error updating service recommendations";
            _logger.LogError(ex, $"{errorMessage}.\nRequestId: {requestId}.\nDetail: {ex.Message}");

            return await CreateErrorResponseAsync(request, HttpStatusCode.InternalServerError, errorMessage, requestId);
        }
    }

    private static async Task<HttpResponseData> CreateErrorResponseAsync(
        HttpRequestData req, 
        HttpStatusCode statusCode, 
        string message, 
        string requestId,
        object? errorDetail = null)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new ApiResponse<object>
        {
            Error = message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow,
            ErrorDetails = errorDetail
        });
        return response;
    }
}
