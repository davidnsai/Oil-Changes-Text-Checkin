using System;
using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Data.Entities
{    public class CheckInSession
    {
        [Key]
        public Guid Id { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }
        public string Payload { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        
        // Navigation properties
        public Customer? Customer { get; set; }

    }

}
