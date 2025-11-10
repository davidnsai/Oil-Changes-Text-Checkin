using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ICheckInRepository
    {
        List<CheckIn> GetAllCheckIns();
        List<CheckIn> GetRecentUnprocessedCheckIns();
        List<CheckIn> GetRecentUnprocessedCheckInsByLocation(Guid locationId);
        CheckIn? GetCheckInById(int id);
        bool AddCheckIn(CheckIn checkIn);
        bool UpdateCheckIn(CheckIn checkIn);
        bool DeleteCheckIn(int id);
        
        Task<CheckIn?> GetCheckInByUuidAsync(Guid uuid);
        
        Task<CheckIn> UpdateCheckInAsync(CheckIn checkIn);
    }
}
