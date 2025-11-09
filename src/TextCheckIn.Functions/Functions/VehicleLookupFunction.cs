using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TextCheckIn.Data.Repositories.Interfaces;
using TextCheckIn.Functions.Models.Responses;
using TextCheckIn.Functions.Models;
using TextCheckIn.Core.Services.Interfaces;
using TextCheckIn.Core.Helpers;
using System.Linq;
using System.Web;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Functions.Functions;

public class VehicleLookupFunction
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ISessionManagementService _sessionManagementService;
    private readonly ILogger<VehicleLookupFunction> _logger;

    public VehicleLookupFunction(
        IVehicleRepository vehicleRepository,
        ISessionManagementService sessionManagementService,
        ILogger<VehicleLookupFunction> logger)
    {
        _vehicleRepository = vehicleRepository;
        _sessionManagementService = sessionManagementService;
        _logger = logger;
    }

    [Function("GetVehicleByLicensePlate")]
    public async Task<HttpResponseData> GetVehicleByLicensePlateAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vehicle-lookup/license-plate/{licensePlate}/{stateCode}")] HttpRequestData req,
        string licensePlate,
        string stateCode)
    {
        var requestId = Guid.NewGuid().ToString()[..8];

        try
        {
            if (string.IsNullOrWhiteSpace(licensePlate) || string.IsNullOrWhiteSpace(stateCode))
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "licensePlate and stateCode are required", requestId);
            }

            // Get check in from query string
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var checkInIdParamx = query["check-in"];
            
            //if (string.IsNullOrWhiteSpace(checkInIdParam))
            //{
            //    return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "location query parameter is required", requestId);
            //}

            //if (!Guid.TryParse(checkInIdParam, out var checkInId))
            //{
            //    return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "check-in must be a valid GUID", requestId);
            //}

            licensePlate = licensePlate.Trim();
            stateCode = stateCode.Trim().ToUpperInvariant();

            _logger.LogInformation("VehicleLookup {RequestId}: Lookup by plate {LicensePlate}/{StateCode} at location {LocationId}", 
                requestId, licensePlate, stateCode, licensePlate);

            var vehicle = _vehicleRepository.GetVehicleByLicensePlateAndStateWithUnprocessedCheckIn(licensePlate, stateCode, Guid.NewGuid());
            if (vehicle == null)
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.NotFound, "Vehicle not found with unprocessed check-in at this location", requestId);
            }

            // mask phone number, email, and first name and last name
            var vehicleResponse = CreateVehicleResponse(vehicle);
            foreach (var customer in vehicleResponse.Customers)
            {
                customer.PhoneNumber = PhoneNumberHelper.MaskPhoneNumber(customer.PhoneNumber);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new ApiResponse<VehicleResponse>
            {
                Success = true,
                Data = vehicleResponse,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VehicleLookup {RequestId}: Error during plate lookup", requestId);
            return await CreateErrorResponseAsync(req, HttpStatusCode.InternalServerError, "Internal server error", requestId);
        }
    }

    [Function("GetVehicleByVin")]
    public async Task<HttpResponseData> GetVehicleByVinAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vehicle-lookup/vin/{vin}")] HttpRequestData req,
        string vin)
    {
        var requestId = Guid.NewGuid().ToString()[..8];

        try
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "vin is required", requestId);
            }

            // Get location from query string
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var locationParam = query["location"];
            
            if (string.IsNullOrWhiteSpace(locationParam))
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "location query parameter is required", requestId);
            }

            if (!Guid.TryParse(locationParam, out var locationId))
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "location must be a valid GUID", requestId);
            }

            vin = vin.Trim();
            _logger.LogInformation("VehicleLookup {RequestId}: Lookup by VIN {Vin} at location {LocationId}", requestId, vin, locationId);

            var vehicle = _vehicleRepository.GetVehicleByVinWithUnprocessedCheckIn(vin, locationId);
            if (vehicle == null)
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.NotFound, "Vehicle not found with unprocessed check-in at this location", requestId);
            }

            // Get the unprocessed check-in for this location
            var checkIn = vehicle.CheckIns.FirstOrDefault(c => !c.IsProcessed && c.OmnixLocationId == locationId);
            if (checkIn == null)
            {
                return await CreateErrorResponseAsync(req, HttpStatusCode.NotFound, "No unprocessed check-in found for this vehicle at location", requestId);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new ApiResponse<VehicleResponse>
            {
                Success = true,
                Data = CreateVehicleResponse(vehicle),
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                SessionId = _sessionManagementService.CurrentSession?.Id.ToString() ?? string.Empty
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VehicleLookup {RequestId}: Error during VIN lookup", requestId);
            return await CreateErrorResponseAsync(req, HttpStatusCode.InternalServerError, "Internal server error", requestId);
        }
    }

    private static async Task<HttpResponseData> CreateErrorResponseAsync(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string message,
        string requestId)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new ApiResponse<object>
        {
            Success = false,
            Error = message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        });
        return response;
    }

    private VehicleResponse CreateVehicleResponse(Vehicle vehicle)
    {
        var vehicleResponse = new VehicleResponse
        {
            Id = vehicle.VehicleUUID,
            Vin = vehicle.Vin,
            LicensePlate = vehicle.LicensePlate,
            StateCode = vehicle.StateCode,
            Make = vehicle.Make,
            Model = vehicle.Model,
            YearOfMake = vehicle.YearOfMake,
            LastMileage = vehicle.LastMileage,
            Customers = new List<CustomerResponse>()
        };

        // Add customers from CustomersVehicles relationship
        foreach (var customerVehicle in vehicle.CustomersVehicles)
        {
            if (customerVehicle.Customer != null)
            {
                var customerResponse = new CustomerResponse
                {
                    Id = customerVehicle.Customer.Uuid,
                    FirstName = customerVehicle.Customer.FirstName,
                    LastName = customerVehicle.Customer.LastName,
                    Email = customerVehicle.Customer.Email,
                    PhoneNumber = customerVehicle.Customer.PhoneNumber,
                    IsFleetCustomer = customerVehicle.Customer.IsFleetCustomer
                };

                // Mask phone number if check-in doesn't have a customer (user not logged in)
                // if (!checkIn.CustomerId.HasValue && !string.IsNullOrWhiteSpace(customerResponse.PhoneNumber))
                // {
                //     customerResponse.PhoneNumber = PhoneNumberHelper.MaskPhoneNumber(customerResponse.PhoneNumber);
                // }

                vehicleResponse.Customers.Add(customerResponse);
            }
        }

        return vehicleResponse;
    }
}

