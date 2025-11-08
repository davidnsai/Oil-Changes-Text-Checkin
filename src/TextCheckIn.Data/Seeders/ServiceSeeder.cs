using TextCheckIn.Data.Entities;
using TextCheckIn.Shared.Models;

namespace TextCheckIn.Data.Seeders
{
    public static class ServiceSeeder
    {
        /// <summary>
        /// Gets seed data for Power-6 services to be used with HasData in EF Core migrations
        /// </summary>
        /// <returns>Array of Service entities with fixed IDs for seeding</returns>
        public static Service[] GetSeedData()
        {
            // Use a fixed timestamp for consistency across migrations
            var seedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            var services = new List<Service>();
            var id = 1;
            
            foreach (var power6Service in Power6ServiceCatalog.Services)
            {
                services.Add(new Service
                {
                    Id = id++,
                    ServiceUuid = Guid.Parse(power6Service.Id),
                    Name = power6Service.Name,
                    Description = power6Service.Description,
                    Price = power6Service.Price,
                    EstimatedDurationMinutes = power6Service.EstimatedDurationMinutes,
                    CreatedAt = seedTimestamp,
                    UpdatedAt = seedTimestamp
                });
            }
            
            return services.ToArray();
        }
    }
}
