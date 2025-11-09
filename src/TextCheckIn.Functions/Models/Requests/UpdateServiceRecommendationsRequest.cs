using System.ComponentModel.DataAnnotations;

namespace TextCheckIn.Functions.Models.Requests
{
    public class UpdateServiceRecommendationsRequest
    {
        [Required(ErrorMessage = "Check-in UUID is required")]
        public required Guid CheckInUuid { get; set; }

        [Required(ErrorMessage = "Services list is required")]
        [MinLength(1, ErrorMessage = "At least one service must be provided")]
        public required List<ServiceSelection> Services { get; set; }
    }

    public class ServiceSelection
    {
        [Required(ErrorMessage = "Service UUID is required")]
        public required Guid ServiceUuid { get; set; }

        [Required(ErrorMessage = "IsCustomerSelected is required")]
        public required bool IsCustomerSelected { get; set; }
    }
}
