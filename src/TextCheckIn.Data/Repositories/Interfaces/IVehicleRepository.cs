using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface IVehicleRepository
    {
        List<Vehicle> GetAllVehicles();
        Vehicle? GetVehicleById(int id);
        Vehicle? GetVehicleByLicensePlateAndState(string licensePlate, string stateCode);
        Vehicle? GetVehicleByVin(string vin);
        Vehicle? GetVehicleByLicensePlateAndStateWithUnprocessedCheckIn(string licensePlate, string stateCode, Guid checkInId);
        Vehicle? GetVehicleByVinWithUnprocessedCheckIn(string vin, Guid checkInId);
        bool AddVehicle(Vehicle vehicle);
        bool UpdateVehicle(Vehicle vehicle);
        bool DeleteVehicle(int id);
    }
}
