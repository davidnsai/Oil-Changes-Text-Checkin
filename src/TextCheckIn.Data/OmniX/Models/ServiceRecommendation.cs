namespace TextCheckIn.Data.OmniX.Models;

/// <summary>
/// Represents the services recommended by OmniX for a particular vehicle.
/// </summary>
public class ServiceRecommendation : OmniXVehicleRequest
{
    public Guid Id { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public int? LastServiceMileage { get; set; }
    public List<ServiceInterval> ServiceIntervals { get; set; } = [];
}
