using System.Text.Json.Serialization;

namespace TextCheckIn.Data.OmniX.Models;

public class WorkOrder : OmniXVehicleRequest
{
    public Guid RecommendationId { get; set; }
    public int? ActualMileage { get; set; }
    public List<RecommendedService> Services { get; set; } = [];
    
}

