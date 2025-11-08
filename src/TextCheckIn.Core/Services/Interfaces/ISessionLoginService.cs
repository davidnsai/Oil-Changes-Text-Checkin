using TextCheckIn.Core.Models.Domain;

namespace TextCheckIn.Core.Services.Interfaces;

/// <summary>
/// Service for handling session login operations
/// </summary>
public interface ISessionLoginService
{
    /// <summary>
    /// Perform login using phone number and store ID
    /// </summary>
    /// <param name="phoneNumber">Customer phone number</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created or existing session</returns>
    Task<CheckInSession> LoginAsync(
        string phoneNumber, 
        string storeId, 
        CancellationToken cancellationToken = default);
}
