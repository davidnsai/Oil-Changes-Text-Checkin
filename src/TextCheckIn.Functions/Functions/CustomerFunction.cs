using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Functions.Models.Requests;
using TextCheckIn.Functions.Models.Responses;

namespace TextCheckIn.Functions.Functions
{
    /// <summary>
    /// Azure Functions for customer management
    /// </summary>
    public class CustomerFunction
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ILogger<CustomerFunction> _logger;

        public CustomerFunction(
            ICustomerRepository customerRepository,
            ISessionManagementService sessionManagementService,
            ILogger<CustomerFunction> logger)
        {
            _customerRepository = customerRepository;
            _sessionManagementService = sessionManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves customer details
        /// GET /api/customer
        /// </summary>
        [Function("GetCustomer")]
        public async Task<HttpResponseData> GetCustomerAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customer")]
            HttpRequestData request)
        {
            var requestId = Guid.NewGuid().ToString()[..8];

            _logger.LogDebug("GetCustomer {RequestId}: Received request", requestId);

            try
            {
                // Check if session has a customer
                if (_sessionManagementService.CurrentSession?.CustomerId == null)
                {
                    _logger.LogWarning("GetCustomer {RequestId}: No customer linked to session", requestId);

                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "No customer linked to this session",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                // Get the customer from the session
                var customer = await _customerRepository.GetByIdAsync(_sessionManagementService.CurrentSession.CustomerId!.Value);

                if (customer == null)
                {
                    _logger.LogWarning("GetCustomer {RequestId}: Customer not found for ID {CustomerId}",
                        requestId, _sessionManagementService.CurrentSession.CustomerId);

                    var notFoundResponse = request.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "Customer not found",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return notFoundResponse;
                }

                _logger.LogInformation("GetCustomer {RequestId}: Successfully retrieved customer {CustomerId}",
                    requestId, customer.Id);

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<CustomerResponse>
                {
                    Success = true,
                    Data = new CustomerResponse
                    {
                        Id = customer.Uuid,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        IsFleetCustomer = customer.IsFleetCustomer
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomer {RequestId}: Error processing request", requestId);

                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
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
        /// Updates customer details
        /// PUT /api/customer
        /// </summary>
        [Function("UpdateCustomer")]
        public async Task<HttpResponseData> UpdateCustomerAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customer")]
            HttpRequestData request)
        {
            var requestId = Guid.NewGuid().ToString()[..8];

            _logger.LogDebug("UpdateCustomer {RequestId}: Received request", requestId);

            try
            {
                // Check if session has a customer
                if (_sessionManagementService.CurrentSession?.CustomerId == null)
                {
                    _logger.LogWarning("UpdateCustomer {RequestId}: No customer linked to session", requestId);
                    
                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "No customer linked to this session",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                // Parse and validate request body
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var updateRequest = JsonSerializer.Deserialize<UpdateCustomerRequest>(request.Body, options);

                if (updateRequest == null)
                {
                    _logger.LogWarning("UpdateCustomer {RequestId}: Invalid request body", requestId);
                    
                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "Invalid request format",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                // Validate request data
                var validationContext = new ValidationContext(updateRequest);
                var validationResults = new List<ValidationResult>();
                
                if (!Validator.TryValidateObject(updateRequest, validationContext, validationResults, true))
                {
                    _logger.LogWarning("UpdateCustomer {RequestId}: Validation failed", requestId);
                    
                    var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "Invalid request data",
                        ErrorDetails = validationResults.Select(vr => new { vr.ErrorMessage, vr.MemberNames }),
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badRequestResponse;
                }

                // Get the customer from the session
                var customer = await _customerRepository.GetByIdAsync(_sessionManagementService.CurrentSession.CustomerId!.Value);
                
                if (customer == null)
                {
                    _logger.LogWarning("UpdateCustomer {RequestId}: Customer not found for ID {CustomerId}", 
                        requestId, _sessionManagementService.CurrentSession.CustomerId);
                    
                    var notFoundResponse = request.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Data = null,
                        Error = "Customer not found",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return notFoundResponse;
                }

                // Update customer details
                customer.FirstName = updateRequest.FirstName;
                customer.LastName = updateRequest.LastName;
                customer.Email = updateRequest.Email;

                var updatedCustomer = await _customerRepository.UpdateCustomerAsync(customer);

                _logger.LogInformation("UpdateCustomer {RequestId}: Successfully updated customer {CustomerId}", 
                    requestId, updatedCustomer.Id);

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponse<CustomerResponse>
                {
                    Success = true,
                    Data = new CustomerResponse
                    {
                        Id = updatedCustomer.Uuid,
                        FirstName = updatedCustomer.FirstName,
                        LastName = updatedCustomer.LastName,
                        Email = updatedCustomer.Email,
                        PhoneNumber = updatedCustomer.PhoneNumber,
                        IsFleetCustomer = updatedCustomer.IsFleetCustomer
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCustomer {RequestId}: Error processing request", requestId);
                
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
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
    }
}
