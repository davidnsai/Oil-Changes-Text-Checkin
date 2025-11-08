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
        
        /// <summary>
        /// Gets a check-in by its UUID
        /// </summary>
        /// <param name="uuid">The check-in UUID</param>
        /// <returns>The check-in if found, null otherwise</returns>
        Task<CheckIn?> GetCheckInByUuidAsync(Guid uuid);
        
        /// <summary>
        /// Updates a check-in asynchronously
        /// </summary>
        /// <param name="checkIn">The check-in to update</param>
        /// <returns>The updated check-in</returns>
        Task<CheckIn> UpdateCheckInAsync(CheckIn checkIn);

        // <summary>
        // Get active session by phone number and store ID
        // </summary>
        //Task<CheckInSession?> GetActiveSessionByPhoneAsync(
        //    string phoneNumber, 
        //    string storeId, 
        //    CancellationToken cancellationToken = default);

        // <summary>
        // Save check-in session
        // </summary>
        //Task<CheckInSession> SaveSessionAsync(
        //    CheckInSession session,
        //    CancellationToken cancellationToken = default);

        // <summary>
        // Update check-in session
        // </summary>
        //Task<CheckInSession> UpdateSessionAsync(
        //    CheckInSession session, 
        //    CancellationToken cancellationToken = default);
    }
}
