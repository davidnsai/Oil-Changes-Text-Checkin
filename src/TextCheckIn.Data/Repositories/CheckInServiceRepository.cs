using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class CheckInServiceRepository : ICheckInServiceRepository
    {
        private readonly AppDbContext _context;

        public CheckInServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<CheckInService> GetAllCheckInServices()
        {
            return _context.CheckInServices
                .Include(cs => cs.CheckIn)
                .Include(cs => cs.Service)
                .ToList();
        }

        public async Task<List<CheckInService>> GetCheckInServicesByCheckInUuidAsync(Guid checkInUuid)
        {
            return await _context.CheckInServices
                .Include(cs => cs.Service)
                .Where(cs => cs.CheckIn.Uuid == checkInUuid)
                .ToListAsync();
        }

        public CheckInService? GetCheckInServiceById(int id)
        {
            return _context.CheckInServices
                .Include(cs => cs.CheckIn)
                .Include(cs => cs.Service)
                .FirstOrDefault(cs => cs.Id == id);
        }

        public bool AddCheckInService(CheckInService checkInService)
        {
            _context.CheckInServices.Add(checkInService);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateCheckInService(CheckInService checkInService)
        {
            _context.CheckInServices.Update(checkInService);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteCheckInService(int id)
        {
            var checkInService = _context.CheckInServices.Find(id);
            if (checkInService == null) return false;
            
            _context.CheckInServices.Remove(checkInService);
            return _context.SaveChanges() > 0;
        }

        public async Task<bool> UpdateCheckInServicesAsync(Guid checkInUuid, Dictionary<Guid, bool> serviceSelections)
        {
            try
            {
                // Get all CheckInServices for the specified check-in UUID and service UUIDs
                var checkInServices = await _context.CheckInServices
                    .Include(cs => cs.CheckIn)
                    .Include(cs => cs.Service)
                    .Where(cs => cs.CheckIn.Uuid == checkInUuid && 
                                serviceSelections.Keys.Contains(cs.Service.ServiceUuid))
                    .ToListAsync();

                if (!checkInServices.Any())
                {
                    return false; // No matching services found
                }

                // Update the IsCustomerSelected field for each service
                foreach (var checkInService in checkInServices)
                {
                    if (serviceSelections.TryGetValue(checkInService.Service.ServiceUuid, out bool isSelected))
                    {
                        checkInService.IsCustomerSelected = isSelected;
                    }
                }

                // Save changes
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
