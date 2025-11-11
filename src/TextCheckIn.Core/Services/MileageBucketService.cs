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
        var buckets = availableBuckets.OrderBy(b => b).ToList();
        
        if (buckets.Count == 0)
        {
            throw new ArgumentException("Available buckets list cannot be empty", nameof(availableBuckets));
        }

        // Determine the target mileage to use for selection
        int? targetMileage = actualMileage ?? estimatedMileage;

        // If no mileage information is available, return the lowest bucket
        if (!targetMileage.HasValue)
        {
            return buckets[0];
        }

        // Find the highest bucket that is <= target mileage
        int? selectedBucket = null;
        foreach (var bucket in buckets)
        {
            if (bucket <= targetMileage.Value)
            {
                selectedBucket = bucket;
            }
            else
            {
                break; // Buckets are sorted, so we can stop here
            }
        }

        // Return the selected bucket, or the lowest if none found
        return selectedBucket ?? buckets[0];
    }
}

