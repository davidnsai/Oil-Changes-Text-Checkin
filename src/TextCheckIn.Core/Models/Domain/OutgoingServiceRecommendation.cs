using TextCheckIn.Shared.Models;

namespace TextCheckIn.Core.Models.Domain;

public class OutgoingServiceRecommendation
{
  
    public int? EstimatedMileage { get; set; }

    public int? ActualMileage { get; set; }

    public int SelectedMileageBucket { get; set; }

    public IEnumerable<int> AvailableBuckets { get; set; } = [];

    public IEnumerable<ServiceRecommendationDetails> RecommendedServices { get; set; } = [];

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public RecommendationSource Source { get; set; }
}

public class ServiceRecommendationDetails
{
    public required string ServiceId { get; set; }

    public required string ServiceName { get; set; }

    public required int IntervalMiles { get; set; }

    public int? LastServiceMiles { get; set; }

    public int? OverdueMiles { get; set; }

    public ServiceUrgency Urgency { get; set; }

    public Power6Service? ServiceDetails { get; set; }

    public void CalculateUrgency(int currentMileage)
    {
        if (LastServiceMiles.HasValue)
        {
            var milesSinceLastService = currentMileage - LastServiceMiles.Value;
            OverdueMiles = Math.Max(0, milesSinceLastService - IntervalMiles);
            
            if (OverdueMiles > 5000)
                Urgency = ServiceUrgency.Overdue;
            else if (OverdueMiles > 0)
                Urgency = ServiceUrgency.Due;
            else if (milesSinceLastService > IntervalMiles * 0.8)
                Urgency = ServiceUrgency.Recommended;
            else
                Urgency = ServiceUrgency.Optional;
        }
        else
        {
            // No service history, determine based on interval
            if (currentMileage >= IntervalMiles)
                Urgency = ServiceUrgency.Due;
            else if (currentMileage >= IntervalMiles * 0.9)
                Urgency = ServiceUrgency.Recommended;
            else
                Urgency = ServiceUrgency.Optional;
        }
    }
}

public enum ServiceUrgency
{
    Optional,

    Recommended,

    Due,

    Overdue
}

public enum RecommendationSource
{
    OmniXWebhook,

    OmniXApi,

    Mock
}
