using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class CheckInRepository : ICheckInRepository
    {
        private readonly AppDbContext _context;

        public CheckInRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public List<CheckIn> GetAllCheckIns()
        {
            return _context.CheckIns
                .Include(c => c.Vehicle)
                .Include(c => c.CheckInServices)
                    .ThenInclude(cs => cs.Service)
                .ToList();
        }

        // these are the check ins that have not been processed yet
        public List<CheckIn> GetRecentUnprocessedCheckIns()
        {
            return _context.CheckIns
                .Include(c => c.Vehicle)
                .Include(c => c.CheckInServices)
                    .ThenInclude(cs => cs.Service)
                .Where(c => !c.IsProcessed)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        // these are the check ins that have not been processed yet for a specific location
        public List<CheckIn> GetRecentUnprocessedCheckInsByLocation(Guid locationId)
        {
            return _context.CheckIns
                .Include(c => c.Vehicle)
                .Include(c => c.CheckInServices)
                    .ThenInclude(cs => cs.Service)
                .Where(c => !c.IsProcessed && c.OmnixLocationId == locationId && c.Vehicle != null)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public CheckIn? GetCheckInById(int id)
        {
            return _context.CheckIns
                .Include(c => c.Vehicle)
                .Include(c => c.CheckInServices)
                    .ThenInclude(cs => cs.Service)
                .FirstOrDefault(c => c.Id == id);
        }

        public bool AddCheckIn(CheckIn checkIn)
        {
            _context.CheckIns.Add(checkIn);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateCheckIn(CheckIn checkIn)
        {
            _context.CheckIns.Update(checkIn);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteCheckIn(int id)
        {
            var checkIn = _context.CheckIns.Find(id);
            if (checkIn == null) return false;
            
            _context.CheckIns.Remove(checkIn);
            return _context.SaveChanges() > 0;
        }

        public async Task<CheckIn?> GetCheckInByUuidAsync(Guid uuid)
        {
            return await _context.CheckIns
                .Include(c => c.Vehicle)
                .Include(c => c.Customer)
                .Include(c => c.CheckInServices)
                    .ThenInclude(cs => cs.Service)
                .FirstOrDefaultAsync(c => c.Uuid == uuid);
        }

        public async Task<CheckIn> UpdateCheckInAsync(CheckIn checkIn)
        {
            checkIn.UpdatedAt = DateTime.UtcNow;
            _context.CheckIns.Update(checkIn);
            await _context.SaveChangesAsync();
            return checkIn;
        }
    }
}