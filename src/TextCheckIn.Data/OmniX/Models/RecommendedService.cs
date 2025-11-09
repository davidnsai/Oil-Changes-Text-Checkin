namespace TextCheckIn.Data.OmniX.Models;

public class RecommendedService
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required int IntervalMiles { get; set; }

    public int? LastServiceMiles { get; set; }

    public bool SelectedByClient { get; set; }
}