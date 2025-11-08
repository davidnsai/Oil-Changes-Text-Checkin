
namespace TextCheckIn.Data.OmniX.Models;

public class ServiceInterval
{
    /// <summary>
    /// Mileage bucket value (required if serviceIntervals is not null)
    /// </summary>
    public required int Mileage { get; set; }

    /// <summary>
    /// List of recommended services for this mileage bucket (required if serviceIntervals is not null)
    /// </summary>
    public required List<RecommendedService> Services { get; set; } = [];
}

