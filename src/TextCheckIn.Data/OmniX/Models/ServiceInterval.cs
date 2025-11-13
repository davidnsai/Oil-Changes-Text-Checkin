using TextCheckIn.Shared.Models;

namespace TextCheckIn.Data.OmniX.Models;

public class ServiceInterval : BaseServiceInterval
{
    public required List<RecommendedService> Services { get; set; } = [];
}

