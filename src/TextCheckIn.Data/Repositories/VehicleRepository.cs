using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Vehicle> GetAllVehicles()
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                .ToList();
        }

        public Vehicle? GetVehicleById(int id)
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                .FirstOrDefault(v => v.Id == id);
        }

        public Vehicle? GetVehicleByLicensePlateAndState(string licensePlate, string stateCode)
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                .FirstOrDefault(v => v.LicensePlate == licensePlate && v.StateCode != null && v.StateCode.ToUpper() == stateCode.ToUpper());
        }

        public Vehicle? GetVehicleByVin(string vin)
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                .FirstOrDefault(v => v.Vin == vin);
        }

        public bool AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteVehicle(int id)
        {
            var vehicle = _context.Vehicles.Find(id);
            if (vehicle == null) return false;
            
            _context.Vehicles.Remove(vehicle);
            return _context.SaveChanges() > 0;
        }

        public Vehicle? GetVehicleByLicensePlateAndStateWithUnprocessedCheckIn(string licensePlate, string stateCode, Guid checkInId)
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                    .ThenInclude(c => c.Customer)
                .Include(v => v.CustomersVehicles)
                    .ThenInclude(cv => cv.Customer)
                .FirstOrDefault(v => 
                    v.LicensePlate == licensePlate && 
                    v.StateCode!.ToUpper() == stateCode.ToUpper() &&
                    v.CheckIns.Any(c => !c.IsProcessed && c.Uuid == checkInId));
        }

        public Vehicle? GetVehicleByVinWithUnprocessedCheckIn(string vin, Guid checkInId)
        {
            return _context.Vehicles
                .Include(v => v.CheckIns)
                    .ThenInclude(c => c.Customer)
                .Include(v => v.CustomersVehicles)
                    .ThenInclude(cv => cv.Customer)
                .FirstOrDefault(v => 
                    v.Vin == vin &&
                    v.CheckIns.Any(c => !c.IsProcessed && c.Uuid == checkInId));
        }
    }
}
