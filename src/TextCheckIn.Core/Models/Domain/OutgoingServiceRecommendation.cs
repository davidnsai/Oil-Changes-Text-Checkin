using TextCheckIn.Shared.Models;

namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents processed service recommendations for a vehicle
/// </summary>
public class OutgoingServiceRecommendation
{
  
    /// <summary>
    /// Vehicle's estimated current mileage
    /// </summary>
    public int? EstimatedMileage { get; set; }

    /// <summary>
    /// Customer's actual odometer reading (if provided)
    /// </summary>
    public int? ActualMileage { get; set; }

    /// <summary>
    /// Selected mileage bucket based on logic
    /// </summary>
    public int SelectedMileageBucket { get; set; }

    /// <summary>
    /// Available mileage buckets from omniX
    /// </summary>
    public IEnumerable<int> AvailableBuckets { get; set; } = [];

    /// <summary>
    /// Recommended services for the selected mileage bucket
    /// </summary>
    public IEnumerable<ServiceRecommendationDetails> RecommendedServices { get; set; } = [];

    /// <summary>
    /// When these recommendations were generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source of recommendations (omniX webhook, API, or mock)
    /// </summary>
    public RecommendationSource Source { get; set; }
}

/// <summary>
/// Detailed information about a recommended service
/// </summary>
public class ServiceRecommendationDetails
{
    /// <summary>
    /// Power-6 service UUID
    /// </summary>
    public required string ServiceId { get; set; }

    /// <summary>
    /// Service name from omniX
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Recommended service interval mileage
    /// </summary>
    public required int IntervalMiles { get; set; }

    /// <summary>
    /// Last time this service was performed (if known)
    /// </summary>
    public int? LastServiceMiles { get; set; }

    /// <summary>
    /// How many miles overdue (if applicable)
    /// </summary>
    public int? OverdueMiles { get; set; }

    /// <summary>
    /// Service urgency level
    /// </summary>
    public ServiceUrgency Urgency { get; set; }

    /// <summary>
    /// Service details from Power-6 catalog
    /// </summary>
    public Power6Service? ServiceDetails { get; set; }

    /// <summary>
    /// Calculate service urgency based on mileage
    /// </summary>
    /// <param name="currentMileage">Current vehicle mileage</param>
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

/// <summary>
/// Service urgency levels for customer guidance
/// </summary>
public enum ServiceUrgency
{
    /// <summary>
    /// Service is optional at current mileage
    /// </summary>
    Optional,

    /// <summary>
    /// Service is recommended soon
    /// </summary>
    Recommended,

    /// <summary>
    /// Service is due now
    /// </summary>
    Due,

    /// <summary>
    /// Service is overdue and should be performed immediately
    /// </summary>
    Overdue
}

/// <summary>
/// Source of service recommendations
/// </summary>
public enum RecommendationSource
{
    /// <summary>
    /// Recommendations from omniX webhook
    /// </summary>
    OmniXWebhook,

    /// <summary>
    /// Recommendations from omniX API fallback
    /// </summary>
    OmniXApi,

    /// <summary>
    /// Mock recommendations for development
    /// </summary>
    Mock
}
