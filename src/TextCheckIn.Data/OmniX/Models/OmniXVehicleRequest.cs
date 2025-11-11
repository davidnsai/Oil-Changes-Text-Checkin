namespace TextCheckIn.Data.OmniX.Models;

public abstract class OmniXVehicleRequest
{
    public Guid LocationId { get; set; }

    public required string ClientLocationId { get; set; }

    public required string Datetime { get; set; }

    public required string LicensePlate { get; set; }

    public required string StateCode { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public string? Vin { get; set; }

    public int? EstimatedMileage { get; set; }
}
