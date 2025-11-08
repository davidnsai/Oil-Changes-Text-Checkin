using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class Service
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid ServiceUuid { get; set; }
    
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
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<CheckInService> CheckInServices { get; set; } = new List<CheckInService>();
}