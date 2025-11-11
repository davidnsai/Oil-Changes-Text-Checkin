namespace TextCheckIn.Core.Services;

public class MileageBucketService
{
    /// <summary>
    /// Selects the appropriate mileage bucket based on actual or estimated mileage.
    /// </summary>
    /// <param name="actualMileage">The actual mileage if available (from CheckIn.ActualMileage or request.Mileage)</param>
    /// <param name="estimatedMileage">The estimated mileage from the check-in</param>
    /// <param name="availableBuckets">The list of available mileage buckets from the service recommendations</param>
    /// <returns>The selected mileage bucket value</returns>
    public int SelectMileageBucket(int? actualMileage, int? estimatedMileage, IEnumerable<int> availableBuckets)
    {
        if (!availableBuckets.Any())
        {
            throw new ArgumentException("Available buckets list cannot be empty", nameof(availableBuckets));
        }

        var buckets = availableBuckets.OrderBy(b => b).ToList();

        // If actual mileage is provided, find the closest bucket that is <= actual mileage
        // For middle cases, round down to the lower bucket
        if (actualMileage.HasValue)
        {
            // Find all buckets <= actual mileage
            var validBuckets = buckets.Where(b => b <= actualMileage.Value).ToList();
            
            if (validBuckets.Any())
            {
                // Return the closest one (which will be the highest value <= actual mileage)
                return validBuckets.Max();
            }
            
            // If no bucket is <= actual mileage, return the lowest available bucket
            return buckets.Min();
        }

        // If no actual mileage, use estimated mileage
        if (!estimatedMileage.HasValue)
        {
            // If no estimated mileage either, return the lowest bucket
            return buckets.Min();
        }

        // Find bucket that exactly matches estimated mileage
        if (buckets.Contains(estimatedMileage.Value))
        {
            return estimatedMileage.Value;
        }

        // If no exact match, find the closest lower bucket
        var lowerBuckets = buckets.Where(b => b <= estimatedMileage.Value).ToList();
        if (lowerBuckets.Any())
        {
            return lowerBuckets.Max();
        }

        // If no bucket is <= estimated mileage, return the lowest available bucket
        return buckets.Min();
    }
}

