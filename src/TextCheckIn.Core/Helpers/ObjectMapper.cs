using TextCheckIn.Data.OmniX.Models;

namespace TextCheckIn.Core.Helpers;

internal static class ObjectMapper
{
    public static ServiceRecommendation? ToServiceRecommendation(Data.OmniX.Models.ServiceRecommendation serviceRecommendation)
    {
        if(serviceRecommendation is null)
        {
            return null;
        }

        return new ServiceRecommendation
        {
            ClientLocationId = serviceRecommendation.ClientLocationId,
            Vin = serviceRecommendation.Vin,
            Datetime = serviceRecommendation.Datetime,
            EstimatedMileage = serviceRecommendation.EstimatedMileage,
            LastServiceDate = serviceRecommendation.LastServiceDate,
            LastServiceMileage = serviceRecommendation.LastServiceMileage,
            LicensePlate = serviceRecommendation.LicensePlate,
            LocationId = serviceRecommendation.LocationId,
            Make = serviceRecommendation.Make,
            Model = serviceRecommendation.Model,
            StateCode = serviceRecommendation.StateCode,
            ServiceIntervals = serviceRecommendation.ServiceIntervals.Select((si) => new ServiceInterval
            {
                Mileage = si.Mileage,
                Services = [.. si.Services.Select(s => new RecommendedService
                {
                    Id = s.Id,
                    IntervalMiles = s.IntervalMiles,
                    LastServiceMiles = s.LastServiceMiles,
                    Name = s.Name,
                })]
            }).ToList(),
            Year = serviceRecommendation.Year
        };
    }
}
