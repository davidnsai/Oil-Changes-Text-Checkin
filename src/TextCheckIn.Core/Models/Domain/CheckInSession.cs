using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Shared.Enums;

namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents an active customer check-in session
/// </summary>
public class CheckInSession
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Oil Changers store location identifier
    /// </summary>
    public required string StoreId { get; set; }

    /// <summary>
    /// License plate number
    /// </summary>
    public required string LicensePlate { get; set; }

    /// <summary>
    /// State code for license plate
    /// </summary>
    public required string StateCode { get; set; }

    /// <summary>
    /// Current step in the check-in process
    /// </summary>
    public CheckInStep CurrentStep { get; set; }

    /// <summary>
    /// Session creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Session last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Session expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Vehicle information from omniX or user input
    /// </summary>
    public VehicleData? VehicleData { get; set; }

    /// <summary>
    /// Customer information collected during check-in
    /// </summary>
    public CustomerData? CustomerData { get; set; }

    /// <summary>
    /// Selected services for this session
    /// </summary>
    public ICollection<SelectedService> SelectedServices { get; set; } = new List<SelectedService>();

    /// <summary>
    /// omniX recommendations received for this vehicle
    /// </summary>
    public List<ServiceRecommendation>? Recommendations { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Check if session is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Check if session is active (not expired and not completed)
    /// </summary>
    public bool IsActive => !IsExpired && CurrentStep != CheckInStep.Completion;
}
