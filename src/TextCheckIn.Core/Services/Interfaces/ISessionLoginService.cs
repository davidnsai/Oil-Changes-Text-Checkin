using TextCheckIn.Core.Models.Domain;

namespace TextCheckIn.Core.Services.Interfaces;

public interface ISessionLoginService
{
    Task<CheckInSession> LoginAsync(
        string phoneNumber, 
        string storeId, 
        CancellationToken cancellationToken = default);
}
