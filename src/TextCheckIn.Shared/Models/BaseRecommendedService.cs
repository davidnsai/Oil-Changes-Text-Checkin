namespace TextCheckIn.Shared.Models;

/// <summary>
/// Base class for service recommendation models with common properties
/// </summary>
public abstract class BaseRecommendedService
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required int IntervalMiles { get; set; }

    public int? LastServiceMiles { get; set; }
}
