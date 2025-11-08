using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Functions.Models.Requests
{
    /// <summary>
    /// Request model for updating service recommendations
    /// </summary>
    public class UpdateServiceRecommendationsRequest
    {
        /// <summary>
        /// The check-in UUID
        /// </summary>
        [Required(ErrorMessage = "Check-in UUID is required")]
        public required Guid CheckInUuid { get; set; }

        /// <summary>
        /// List of services with their selection status
        /// </summary>
        [Required(ErrorMessage = "Services list is required")]
        [MinLength(1, ErrorMessage = "At least one service must be provided")]
        public required List<ServiceSelection> Services { get; set; }
    }

    /// <summary>
    /// Represents a service selection with its customer selection status
    /// </summary>
    public class ServiceSelection
    {
        /// <summary>
        /// The service UUID
        /// </summary>
        [Required(ErrorMessage = "Service UUID is required")]
        public required Guid ServiceUuid { get; set; }

        /// <summary>
        /// Whether the customer selected this service
        /// </summary>
        [Required(ErrorMessage = "IsCustomerSelected is required")]
        public required bool IsCustomerSelected { get; set; }
    }
}
