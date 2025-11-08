namespace TextCheckIn.Core.Models.Domain;

public class VehicleData
{
    public required string LicensePlate { get; set; }

    public required string StateCode { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public string? Color { get; set; }

    public string? Vin { get; set; }

    public int? ActualMileage { get; set; }

    public int? EstimatedMileage { get; set; }

    public DateOnly? LastServiceDate { get; set; }

    public int? LastServiceMileage { get; set; }
}

public enum VehicleDataSource
{
    OmniXWebhook,

    OmniXApi,

    Manual,

    Mock
}
