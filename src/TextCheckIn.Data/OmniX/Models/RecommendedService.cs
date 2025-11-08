namespace TextCheckIn.Data.OmniX.Models;

/// <summary>
/// Represents a service recommended by OmniX.
/// </summary>
public class RecommendedService
{
    /// <summary>
    /// Service UUID identifier (constant, never changes)
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Service display name (may change over time)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Recommended mileage interval for this service
    /// </summary>
    public required int IntervalMiles { get; set; }

    /// <summary>
    /// Last service mileage from history (optional)
    /// </summary>
    public int? LastServiceMiles { get; set; }

    /// <summary>
    /// Whether this service was selected by the client
    /// </summary>
    public bool SelectedByClient { get; set; }
}