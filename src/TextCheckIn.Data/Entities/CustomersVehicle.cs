using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities
{
    public class CustomersVehicle
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int VehicleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
