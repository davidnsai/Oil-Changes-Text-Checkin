using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Service> GetAllServices()
        {
            return _context.Services
                .Include(s => s.CheckInServices)
                .ToList();
        }

        public Service? GetServiceById(int id)
        {
            return _context.Services
                .Include(s => s.CheckInServices)
                .FirstOrDefault(s => s.Id == id);
        }

        public Service? GetServiceByUuid(Guid uuid)
        {
            return _context.Services
                .Include(s => s.CheckInServices)
                .FirstOrDefault(s => s.ServiceUuid == uuid);
        }

        public bool AddService(Service service)
        {
            _context.Services.Add(service);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateService(Service service)
        {
            _context.Services.Update(service);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteService(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null) return false;
            
            _context.Services.Remove(service);
            return _context.SaveChanges() > 0;
        }
    }
}
