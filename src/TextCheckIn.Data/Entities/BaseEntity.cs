using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

/// <summary>
/// Base entity class with common audit fields for all entities
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid Uuid { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
