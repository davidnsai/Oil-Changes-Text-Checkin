using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class StateCode
{
    [Key]
    [StringLength(2)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Store> Stores { get; set; } = new List<Store>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
