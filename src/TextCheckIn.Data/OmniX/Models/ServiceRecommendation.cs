namespace TextCheckIn.Data.OmniX.Models;

public class ServiceRecommendation : OmniXVehicleRequest
{
    public Guid Id { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public int? LastServiceMileage { get; set; }
    public List<ServiceInterval> ServiceIntervals { get; set; } = [];
}
