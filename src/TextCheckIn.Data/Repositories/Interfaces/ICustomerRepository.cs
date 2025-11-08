using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Customer entity operations
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Gets a customer by their UUID
        /// </summary>
        /// <param name="uuid">The customer UUID</param>
        /// <returns>The customer if found, null otherwise</returns>
        Task<Customer?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Gets a customer by their ID
        /// </summary>
        /// <param name="id">The customer ID</param>
        /// <returns>The customer if found, null otherwise</returns>
        Task<Customer?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a customer by their phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to search for</param>
        /// <returns>The customer if found, null otherwise</returns>
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <param name="customer">The customer to create</param>
        /// <returns>The created customer with generated ID</returns>
        Task<Customer> CreateCustomerAsync(Customer customer);

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        /// <param name="customer">The customer to update</param>
        /// <returns>The updated customer</returns>
        Task<Customer> UpdateCustomerAsync(Customer customer);

        /// <summary>
        /// Checks if a customer exists with the given phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to check</param>
        /// <returns>True if customer exists, false otherwise</returns>
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    }
}
