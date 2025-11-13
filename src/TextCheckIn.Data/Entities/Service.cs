using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class Service : BaseEntity
{
    /// <summary>
    /// Service-specific UUID property name for compatibility
    /// </summary>
    public Guid ServiceUuid
    {
        get => Uuid;
        set => Uuid = value;
    }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(10)]
    public decimal? Price { get; set; }
    
    [Required]
    [MaxLength(10)]
    public int? EstimatedDurationMinutes { get; set; }
    
    // Navigation property
    public ICollection<CheckInService> CheckInServices { get; set; } = new List<CheckInService>();
}
