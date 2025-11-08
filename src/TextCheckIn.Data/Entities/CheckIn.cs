using System;
using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class CheckIn
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public Guid Uuid { get; set; }
    
    public int? ClientLocationId { get; set; }

    public Guid? OmnixLocationId { get; set; }
    
    public int? VehicleId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? StoreId { get; set; }
    
    public bool IsProcessed { get; set; }
    
    public int? EstimatedMileage { get; set; }
    
    public DateTime DateTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Vehicle? Vehicle { get; set; }
    public Customer? Customer { get; set; }
    public Store? Store { get; set; }
    public ICollection<CheckInService> CheckInServices { get; set; } = new List<CheckInService>();
}