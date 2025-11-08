namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents a service selected by the customer during check-in
/// </summary>
public class SelectedService
{
    /// <summary>
    /// Power-6 service UUID
    /// </summary>
    public required string ServiceId { get; set; }

    /// <summary>
    /// Service name (for display purposes)
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Service price at time of selection
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    public required int EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Whether this service was recommended by omniX
    /// </summary>
    public bool IsRecommended { get; set; }

    /// <summary>
    /// Mileage at which this service was recommended (if applicable)
    /// </summary>
    public int? RecommendedAtMileage { get; set; }

    /// <summary>
    /// Customer notes about this service
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// When this service was selected
    /// </summary>
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}
