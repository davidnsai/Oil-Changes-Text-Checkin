namespace TextCheckIn.Core.Models.Domain;

public class SelectedService
{
    public required string ServiceId { get; set; }

    public required string ServiceName { get; set; }

    public required decimal Price { get; set; }

    public required int EstimatedDurationMinutes { get; set; }

    public bool IsRecommended { get; set; }

    public int? RecommendedAtMileage { get; set; }

    public string? CustomerNotes { get; set; }

    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}
