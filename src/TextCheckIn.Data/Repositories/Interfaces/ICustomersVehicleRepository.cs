using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for CustomersVehicle entity operations
    /// </summary>
    public interface ICustomersVehicleRepository
    {
        /// <summary>
        /// Gets a customer-vehicle relationship by customer and vehicle IDs
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The relationship if found, null otherwise</returns>
        Task<CustomersVehicle?> GetByCustomerAndVehicleAsync(int customerId, int vehicleId);

        /// <summary>
        /// Creates a new customer-vehicle relationship
        /// </summary>
        /// <param name="customersVehicle">The relationship to create</param>
        /// <returns>The created relationship</returns>
        Task<CustomersVehicle> CreateAsync(CustomersVehicle customersVehicle);

        /// <summary>
        /// Checks if a customer-vehicle relationship exists
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>True if relationship exists, false otherwise</returns>
        Task<bool> ExistsAsync(int customerId, int vehicleId);
    }
}

