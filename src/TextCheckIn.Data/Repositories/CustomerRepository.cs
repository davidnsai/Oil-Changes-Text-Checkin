using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(AppDbContext context, ILogger<CustomerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Customer?> GetByUuidAsync(string uuid)
        {
            try
            {
                var guid = Guid.Parse(uuid);

                return await _context.Customers
                    .Include(c => c.CustomersVehicles)
                    .FirstOrDefaultAsync(c => c.Uuid == guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by UUID {Uuid}", uuid);
                throw;
            }
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.CustomersVehicles)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.CustomersVehicles)
                    .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone number {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            try
            {
                customer.Uuid = Guid.NewGuid();
                customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new customer with ID {CustomerId}", customer.Id);
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.UpdatedAt = DateTime.UtcNow;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated customer with ID {CustomerId}", customer.Id);
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", customer.Id);
                throw;
            }
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                return await _context.Customers
                    .AnyAsync(c => c.PhoneNumber == phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer exists with phone number {PhoneNumber}", phoneNumber);
                throw;
            }
        }
    }
}
