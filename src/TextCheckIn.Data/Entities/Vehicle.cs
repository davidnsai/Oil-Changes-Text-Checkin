using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace TextCheckIn.Data.Entities;

public class Vehicle
{
    [Key]
    public int Id { get; set; }

    public Guid VehicleUUID { get; set; }

    [StringLength(17, MinimumLength = 17)]
    public string? Vin { get; set; }

    [MaxLength(10)]
    public string LicensePlate { get; set; } = string.Empty;

    [StringLength(2)]
    public string? StateCode { get; set; }

    [MaxLength(100)]
    public string? Make { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(4)]
    public int? YearOfMake { get; set; }

    [Range(0, int.MaxValue)]
    public int? LastMileage { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public int? LastServiceMileage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<CustomersVehicle> CustomersVehicles { get; set; } = new List<CustomersVehicle>();
    
    // Navigation properties
    public StateCode? State { get; set; }
}
