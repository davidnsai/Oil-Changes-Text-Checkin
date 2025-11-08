using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Functions.Models.Requests
{
    /// <summary>
    /// Request model for updating customer details
    /// </summary>
    public class UpdateCustomerRequest
    {
        /// <summary>
        /// Customer's first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public required string FirstName { get; set; }

        /// <summary>
        /// Customer's last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public required string LastName { get; set; }

        /// <summary>
        /// Customer's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public required string Email { get; set; }
    }
}
