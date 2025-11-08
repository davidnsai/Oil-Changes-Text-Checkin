using System.Threading.Tasks;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByUuidAsync(string uuid);

        Task<Customer?> GetByIdAsync(int id);

        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);

        Task<Customer> CreateCustomerAsync(Customer customer);

        Task<Customer> UpdateCustomerAsync(Customer customer);

        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    }
}
