using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TextCheckIn.Core.Helpers;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Functions.Models.Requests;
using TextCheckIn.Functions.Models.Responses;

namespace TextCheckIn.Functions.Functions
{
    /// <summary>
    /// Azure Functions for OTP verification
    /// </summary>
    public class OtpVerificationFunction
    {
        private readonly IOtpService _otpService;
        private readonly ISmsService _smsService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ICheckInRepository _checkInRepository;
        private readonly ICustomersVehicleRepository _customersVehicleRepository;
        private readonly ILogger<OtpVerificationFunction> _logger;

        public OtpVerificationFunction(
            IOtpService otpService,
            ISmsService smsService,
            ICustomerRepository customerRepository,
            ISessionManagementService sessionManagementService,
            ICheckInRepository checkInRepository,
            ICustomersVehicleRepository customersVehicleRepository,
            ILogger<OtpVerificationFunction> logger)
        {
            _otpService = otpService;
            _smsService = smsService;
            _customerRepository = customerRepository;
            _sessionManagementService = sessionManagementService;
            _checkInRepository = checkInRepository;
            _customersVehicleRepository = customersVehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Sends OTP to the specified phone number
        /// </summary>
        [Function("SendOtp")]
        public async Task<HttpResponseData> SendOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "otp/send")] HttpRequestData req,
            FunctionContext context)
        {
            var requestId = Guid.NewGuid().ToString();

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var requestBody = await req.ReadAsStringAsync();
                var requestData = JsonSerializer.Deserialize<OtpSendRequest>(requestBody!, options);

                if (requestData == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Error = "Invalid request format",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                Customer? customer = null;

                string normalizedPhone;

                // Check if this is a new customer (ID starts with "new-")
                if (!string.IsNullOrEmpty(requestData.CustomerId) && requestData.CustomerId.StartsWith("new-", StringComparison.OrdinalIgnoreCase))
                {
                    // For new customers, extract and normalize the phone number from the request
                    normalizedPhone = PhoneNumberHelper.FormatToE164(requestData.PhoneNumber);
                    if (string.IsNullOrEmpty(normalizedPhone))
                    {
                        var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                        {
                            Success = false,
                            Data = null,
                            Error = "Invalid phone number format",
                            Timestamp = DateTime.UtcNow,
                            RequestId = requestId,
                            SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                        });
                        return badResponse;
                    }

                    _logger.LogInformation("Processing OTP request for new customer with phone {PhoneNumber}", PhoneNumberHelper.MaskPhoneNumber(normalizedPhone));
                }
                // Check if phone number is masked (format: "(XXX) XXX-1234")
                else if (requestData.PhoneNumber.StartsWith("(XXX)"))
                {
                    // Phone is masked, must have customer ID to look up actual phone number
                    if (string.IsNullOrEmpty(requestData.CustomerId))
                    {
                        var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                        {
                            Success = false,
                            Data = null,
                            Error = "Customer ID required for masked phone numbers",
                            Timestamp = DateTime.UtcNow,
                            RequestId = requestId,
                            SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                        });
                        return badResponse;
                    }

                    customer = await _customerRepository.GetByUuidAsync(requestData.CustomerId);

                    if (customer == null)
                    {
                        var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFoundResponse.WriteAsJsonAsync(new ApiResponse<object>
                        {
                            Success = false,
                            Data = null,
                            Error = "Customer not found",
                            Timestamp = DateTime.UtcNow,
                            RequestId = requestId,
                            SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                        });
                        return notFoundResponse;
                    }

                    // Use the customer's stored phone number
                    normalizedPhone = customer.PhoneNumber;
                    _logger.LogInformation("Processing OTP request for existing customer {CustomerId}", requestData.CustomerId);
                }
                else
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Error = "Unknown operation for provided customer ID and phone number",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                // Get session context to ensure OTP is stored in session
                // var sessionContext = context.GetSessionContext();

                // Generate OTP
                string otpCode;
                try
                {
                    otpCode = await _otpService.GenerateOtpAsync(normalizedPhone);
                }
                catch (InvalidOperationException ex)
                {
                    // Check if this is a cooldown or duplicate request error
                    var cooldownSeconds = await _otpService.GetCooldownRemainingSecondsAsync(normalizedPhone);

                    var rateLimitResponse = req.CreateResponse(HttpStatusCode.TooManyRequests);
                    await rateLimitResponse.WriteAsJsonAsync(new ApiResponse<OtpResponse>
                    {
                        Success = false,
                        Data = new OtpResponse
                        {
                            CooldownSeconds = cooldownSeconds
                        },
                        Error = ex.Message,
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return rateLimitResponse;
                }

                _logger.LogInformation("Remaining attempts: {RemainingAttempts}",
                    await _otpService.GetRemainingAttemptsAsync(normalizedPhone));

                // Send OTP via SMS
                var firstName = customer?.FirstName ?? "";
                var lastName = customer?.LastName ?? "";
                // var smsSent = await _smsService.SendOtpAsync(normalizedPhone, otpCode, firstName, lastName);

                // if (!smsSent)
                // {
                //     var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                //     await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
                //     {
                //         Success = false,
                //         Data = null,
                //         Error = "Failed to send verification code",
                //         Timestamp = DateTime.UtcNow,
                //         RequestId = requestId,
                //         SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                //     });
                //     return errorResponse;
                // }

                // Mask phone number for response
                var maskedPhone = PhoneNumberHelper.MaskPhoneNumber(normalizedPhone);

                _logger.LogInformation("OTP sent successfully to {PhoneNumber}", maskedPhone);

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<OtpResponse>
                {
                    Success = true,
                    Data = new OtpResponse
                    {
                        MaskedPhoneNumber = maskedPhone
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
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
        /// Verifies OTP code
        /// </summary>
        [Function("VerifyOtp")]
        public async Task<HttpResponseData> VerifyOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "otp/verify")] HttpRequestData req,
            FunctionContext context)
        {
            var requestId = Guid.NewGuid().ToString();

            try
            {
                var request = await req.ReadFromJsonAsync<OtpVerifyRequest>();
                if (request == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Error = "Invalid request format",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                // Normalize phone number
                var normalizedPhone = PhoneNumberHelper.FormatToE164(request.PhoneNumber);
                if (string.IsNullOrEmpty(normalizedPhone))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Success = false,
                        Data = null,
                        Error = "Invalid phone number format",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                // Validate OTP
                var isValid = await _otpService.ValidateOtpAsync(normalizedPhone, request.OtpCode);

                if (!isValid)
                {
                    var remainingAttempts = await _otpService.GetRemainingAttemptsAsync(normalizedPhone);
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<OtpResponse>
                    {
                        Success = false,
                        Data = new OtpResponse
                        {
                            RemainingAttempts = remainingAttempts
                        },
                        Error = "Invalid verification code",
                        Timestamp = DateTime.UtcNow,
                        RequestId = requestId,
                        SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
                    });
                    return badResponse;
                }

                // Check if customer exists
                var customer = await _customerRepository.GetByPhoneNumberAsync(normalizedPhone);
                var isNewCustomer = customer == null;

                if (isNewCustomer)
                {
                    // Create new customer with minimal data
                    customer = new Customer
                    {
                        FirstName = "",
                        LastName = "",
                        PhoneNumber = normalizedPhone,
                        IsFleetCustomer = false
                    };

                    customer = await _customerRepository.CreateCustomerAsync(customer);
                    _logger.LogInformation("Created new customer with ID {CustomerId}", customer.Id);
                }

                // Link customer to session
                if (_sessionManagementService.CurrentSession != null)
                {
                    _sessionManagementService.CurrentSession.CustomerId = customer.Id;
                    await _sessionManagementService.UpdateSessionAsync(_sessionManagementService.CurrentSession);
                    _logger.LogInformation("Linked customer {CustomerId} to session {SessionId}", customer.Id, _sessionManagementService.CurrentSession?.Id);
                }

                // Get check-in from session payload if it exists
                CheckIn? checkIn = null;
                if (_sessionManagementService.CurrentSession != null && !string.IsNullOrEmpty(_sessionManagementService.CurrentSession.Payload))
                {
                    try
                    {
                        var payloadData = JsonSerializer.Deserialize<Dictionary<string, object>>(_sessionManagementService.CurrentSession.Payload);
                        if (payloadData != null && payloadData.ContainsKey("checkInId"))
                        {
                            var checkInIdStr = payloadData["checkInId"].ToString();
                            if (Guid.TryParse(checkInIdStr, out var checkInId))
                            {
                                checkIn = await _checkInRepository.GetCheckInByUuidAsync(checkInId);
                                if (checkIn != null)
                                {
                                    // Update check-in with customer ID
                                    checkIn.CustomerId = customer.Id;
                                    await _checkInRepository.UpdateCheckInAsync(checkIn);
                                    _logger.LogInformation("Updated check-in {CheckInId} with customer {CustomerId}", checkIn?.Id, customer.Id);

                                    // If this is a new customer and we have a vehicle, create the customer-vehicle link
                                    if (isNewCustomer && checkIn.VehicleId.HasValue)
                                    {
                                        var existingLink = await _customersVehicleRepository.GetByCustomerAndVehicleAsync(customer.Id, checkIn.VehicleId.Value);
                                        if (existingLink == null)
                                        {
                                            var customersVehicle = new CustomersVehicle
                                            {
                                                CustomerId = customer.Id,
                                                VehicleId = checkIn.VehicleId.Value
                                            };
                                            await _customersVehicleRepository.CreateAsync(customersVehicle);
                                            _logger.LogInformation("Created customer-vehicle link for customer {CustomerId} and vehicle {VehicleId}",
                                                customer.Id, checkIn.VehicleId.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process check-in from session payload");
                    }
                }

                var sessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? Guid.NewGuid().ToString();

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<OtpResponse>
                {
                    Success = true,
                    Data = new OtpResponse
                    {
                        CustomerId = customer?.Uuid,
                        IsNewCustomer = isNewCustomer,
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    SessionId = sessionId
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
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
