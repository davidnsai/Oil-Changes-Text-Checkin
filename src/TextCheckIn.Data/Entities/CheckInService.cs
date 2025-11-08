using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities;

public class CheckInService
{
    [Key]
    public int Id { get; set; }

    public int CheckInId { get; set; }

    public int ServiceId { get; set; }

    public bool IsCustomerSelected { get; set; }

    public int IntervalMiles { get; set; }

    public int? LastServiceMiles { get; set; }

    public int MileageBucket { get; set; }

    // Navigation properties
    public CheckIn CheckIn { get; set; } = null!;
    public Service Service { get; set; } = null!;
}