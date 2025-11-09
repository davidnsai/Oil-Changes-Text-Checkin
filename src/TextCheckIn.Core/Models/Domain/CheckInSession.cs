using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Shared.Enums;

namespace TextCheckIn.Core.Models.Domain;

public class CheckInSession
{
    public required string Id { get; set; }

    public required string StoreId { get; set; }

    public required string LicensePlate { get; set; }

    public required string StateCode { get; set; }

    public CheckInStep CurrentStep { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public VehicleData? VehicleData { get; set; }

    public CustomerData? CustomerData { get; set; }

    public ICollection<SelectedService> SelectedServices { get; set; } = new List<SelectedService>();

    public List<ServiceRecommendation>? Recommendations { get; set; }

    public string? Notes { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsActive => !IsExpired && CurrentStep != CheckInStep.Completion;
}
