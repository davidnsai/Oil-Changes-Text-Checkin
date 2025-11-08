using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ICheckInServiceRepository
    {
        List<CheckInService> GetAllCheckInServices();
        Task<List<CheckInService>> GetCheckInServicesByCheckInUuidAsync(Guid checkInUuid);
        CheckInService? GetCheckInServiceById(int id);
        bool AddCheckInService(CheckInService checkInService);
        bool UpdateCheckInService(CheckInService checkInService);
        bool DeleteCheckInService(int id);
        Task<bool> UpdateCheckInServicesAsync(Guid checkInUuid, Dictionary<Guid, bool> serviceSelections);
    }
}
