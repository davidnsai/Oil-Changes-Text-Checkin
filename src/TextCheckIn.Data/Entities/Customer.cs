using System;
using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public Guid Uuid { get; set; }
        
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
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<CustomersVehicle> CustomersVehicles { get; set; } = new List<CustomersVehicle>();
    }
}
