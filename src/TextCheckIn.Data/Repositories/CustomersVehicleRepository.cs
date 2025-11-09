using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TextCheckIn.Data.Context;
using TextCheckIn.Data.Entities;
using TextCheckIn.Data.Repositories.Interfaces;

namespace TextCheckIn.Data.Repositories
{
    public class CustomersVehicleRepository : ICustomersVehicleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomersVehicleRepository> _logger;

        public CustomersVehicleRepository(AppDbContext context, ILogger<CustomersVehicleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomersVehicle?> GetByCustomerAndVehicleAsync(int customerId, int vehicleId)
        {
            try
            {
                return await _context.CustomersVehicles
                    .Include(cv => cv.Customer)
                    .Include(cv => cv.Vehicle)
                    .FirstOrDefaultAsync(cv => cv.CustomerId == customerId && cv.VehicleId == vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer-vehicle relationship for customer {CustomerId} and vehicle {VehicleId}", customerId, vehicleId);
                throw;
            }
        }

        public async Task<CustomersVehicle> CreateAsync(CustomersVehicle customersVehicle)
        {
            try
            {
                customersVehicle.CreatedAt = DateTime.UtcNow;
                customersVehicle.UpdatedAt = DateTime.UtcNow;

                _context.CustomersVehicles.Add(customersVehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created customer-vehicle relationship for customer {CustomerId} and vehicle {VehicleId}", 
                    customersVehicle.CustomerId, customersVehicle.VehicleId);
                
                return customersVehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer-vehicle relationship");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int customerId, int vehicleId)
        {
            try
            {
                return await _context.CustomersVehicles
                    .AnyAsync(cv => cv.CustomerId == customerId && cv.VehicleId == vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer-vehicle relationship exists for customer {CustomerId} and vehicle {VehicleId}", 
                    customerId, vehicleId);
                throw;
            }
        }
    }
}

