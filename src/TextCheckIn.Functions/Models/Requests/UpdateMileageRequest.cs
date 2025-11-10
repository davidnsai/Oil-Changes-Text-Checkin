using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TextCheckIn.Functions.Converters;

namespace TextCheckIn.Functions.Models.Requests;

public class UpdateMileageRequest
{
    [Required(ErrorMessage = "Mileage is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Mileage must be a positive number")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int Mileage { get; set; }
}

