using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ICustomersVehicleRepository
    {
        Task<CustomersVehicle?> GetByCustomerAndVehicleAsync(int customerId, int vehicleId);

        Task<CustomersVehicle> CreateAsync(CustomersVehicle customersVehicle);

        Task<bool> ExistsAsync(int customerId, int vehicleId);
    }
}

