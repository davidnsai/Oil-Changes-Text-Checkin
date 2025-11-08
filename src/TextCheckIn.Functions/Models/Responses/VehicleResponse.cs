namespace TextCheckIn.Functions.Models.Responses;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public Guid CheckInId { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? StateCode { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? YearOfMake { get; set; }
    public int? LastMileage { get; set; }
    public List<CustomerResponse> Customers { get; set; } = new List<CustomerResponse>();
}

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsFleetCustomer { get; set; }
}

