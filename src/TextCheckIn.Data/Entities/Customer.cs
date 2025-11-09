using System;
using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }
        
        [MaxLength(255)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        public bool IsFleetCustomer { get; set; }
        
        // Navigation properties
        public ICollection<CustomersVehicle> CustomersVehicles { get; set; } = new List<CustomersVehicle>();
    }
}
