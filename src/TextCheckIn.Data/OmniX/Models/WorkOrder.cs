namespace TextCheckIn.Data.OmniX.Models;

/// <summary>
/// Represents a work order for a particular vehicle.
/// </summary>
public class WorkOrder : OmniXVehicleRequest
{
    public Guid RecommendationId { get; set; }
    public int? ActualMileage { get; set; }
    public List<RecommendedService> Services { get; set; } = [];
}

