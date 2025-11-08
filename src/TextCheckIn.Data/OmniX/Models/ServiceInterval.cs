
namespace TextCheckIn.Data.OmniX.Models;

public class ServiceInterval
{
    public required int Mileage { get; set; }

    public required List<RecommendedService> Services { get; set; } = [];
}

