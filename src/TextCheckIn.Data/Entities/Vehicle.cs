using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace TextCheckIn.Data.Entities;

public class Vehicle : BaseEntity
{
    /// <summary>
    /// Vehicle-specific UUID property name for compatibility
    /// </summary>
    public Guid VehicleUUID
    {
        get => Uuid;
        set => Uuid = value;
    }

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

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<CustomersVehicle> CustomersVehicles { get; set; } = new List<CustomersVehicle>();
    
    // Navigation properties
    public StateCode? State { get; set; }
}
