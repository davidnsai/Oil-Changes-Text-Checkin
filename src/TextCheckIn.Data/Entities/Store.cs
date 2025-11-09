using System;
using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class Store : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [StringLength(2)]
    public string? StateCode { get; set; }
    
    [MaxLength(10)]
    public string? ZipCode { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    // Navigation properties
    public StateCode? State { get; set; }
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}