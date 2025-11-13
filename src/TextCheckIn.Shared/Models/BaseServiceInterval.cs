namespace TextCheckIn.Shared.Models;

/// <summary>
/// Base class for service interval models with common properties
/// </summary>
public abstract class BaseServiceInterval
{
    public required int Mileage { get; set; }
}
