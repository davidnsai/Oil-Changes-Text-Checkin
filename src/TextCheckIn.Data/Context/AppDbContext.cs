using Microsoft.EntityFrameworkCore;
using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

    public DbSet<CheckInSession> CheckInSessions { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<CustomersVehicle> CustomersVehicles { get; set; }
    public DbSet<CheckInService> CheckInServices { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StateCode> StateCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names
        modelBuilder.Entity<CheckInSession>().ToTable("check_in_sessions");
        modelBuilder.Entity<Customer>().ToTable("customers");
        modelBuilder.Entity<Service>().ToTable("services");
        modelBuilder.Entity<CheckIn>().ToTable("check_ins");
        modelBuilder.Entity<Vehicle>().ToTable("vehicles");
        modelBuilder.Entity<CustomersVehicle>().ToTable("customers_vehicles");
        modelBuilder.Entity<CheckInService>().ToTable("check_in_services");
        modelBuilder.Entity<Store>().ToTable("stores");
        modelBuilder.Entity<StateCode>().ToTable("state_codes");

        // Configure CheckInSession entity
        modelBuilder.Entity<CheckInSession>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.IpAddress)
                .HasColumnName("ip_address");

            entity.Property(e => e.Payload)
                .HasColumnName("payload");

            entity.Property(e => e.LastActivity)
                .HasColumnName("last_activity");

            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("customer_id");

            entity.Property(e => e.Uuid)
                .HasColumnName("customer_uuid");

            entity.Property(e => e.FirstName)
                .HasColumnName("customer_first_name");

            entity.Property(e => e.LastName)
                .HasColumnName("customer_last_name");

            entity.Property(e => e.Email)
                .HasColumnName("customer_email");

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("customer_phone_number");
            
            entity.Property(e => e.IsFleetCustomer)
                .HasColumnName("is_fleet_customer");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("customer_created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("customer_updated_at");
        });

        // Configure CheckIn entity
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");
                
            entity.Property(e => e.Uuid)
                .HasColumnName("uuid");

            entity.Property(e => e.ClientLocationId)
                .HasColumnName("client_location_id");

            entity.Property(e => e.OmnixLocationId)
                .HasColumnName("omnix_location_id");

            entity.Property(e => e.VehicleId)
                .HasColumnName("vehicle_id");

            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");

            entity.Property(e => e.StoreId)
                .HasColumnName("store_id");

            entity.Property(e => e.IsProcessed)
                .HasColumnName("is_processed");

            entity.Property(e => e.EstimatedMileage)
                .HasColumnName("estimated_mileage");

            entity.Property(e => e.DateTime)
                .HasColumnName("datetime");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.CheckIns)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Store)
                .WithMany(s => s.CheckIns)
                .HasForeignKey(e => e.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Vehicle entity
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.VehicleUUID)
                .HasColumnName("vehicle_uuid");

            entity.Property(e => e.LicensePlate)
                .HasColumnName("license_plate");

            entity.Property(e => e.StateCode)
                .HasColumnName("state_code");

            entity.Property(e => e.Vin)
                .HasColumnName("vin");

            entity.Property(e => e.Make)
                .HasColumnName("make");

            entity.Property(e => e.Model)
                .HasColumnName("model");

            entity.Property(e => e.YearOfMake)
                .HasColumnName("year_of_make");

            entity.Property(e => e.LastMileage)
                .HasColumnName("last_mileage");

            entity.Property(e => e.LastServiceDate)
                .HasColumnName("last_service_date");

            entity.Property(e => e.LastServiceMileage)
                .HasColumnName("last_service_mileage");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.Vin)
                .IsUnique();

            entity.HasIndex(v => new { v.LicensePlate, v.StateCode })
                .IsUnique();

            entity.HasOne(e => e.State)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(e => e.StateCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure CustomersVehicle entity
        modelBuilder.Entity<CustomersVehicle>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");
                
            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");

            entity.Property(e => e.VehicleId)
                .HasColumnName("vehicle_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.CustomersVehicles)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.CustomersVehicles)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CheckInService entity (junction table)
        modelBuilder.Entity<CheckInService>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");
                
            entity.Property(e => e.CheckInId)
                .HasColumnName("check_in_id");

            entity.Property(e => e.ServiceId)
                .HasColumnName("service_id");

            entity.Property(e => e.IsCustomerSelected)
                .HasColumnName("is_customer_selected");

            entity.Property(e => e.IntervalMiles)
                .HasColumnName("interval_miles");

            entity.Property(e => e.LastServiceMiles)
                .HasColumnName("last_service_miles");

            entity.Property(e => e.MileageBucket)
                .HasColumnName("mileage_bucket");

            entity.HasOne(e => e.CheckIn)
                .WithMany(c => c.CheckInServices)
                .HasForeignKey(e => e.CheckInId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.CheckInServices)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Service entity
        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.ServiceUuid)
                .HasColumnName("service_uuid");

            entity.Property(e => e.Name)
                .HasColumnName("name");
            
            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.Price)
                .HasColumnName("price");
            
            entity.Property(e => e.EstimatedDurationMinutes)
                .HasColumnName("estimated_duration_minutes");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            // Seed Power-6 services
            entity.HasData(Seeders.ServiceSeeder.GetSeedData());
        });

        // Configure StateCode entity
        modelBuilder.Entity<StateCode>(entity =>
        {
            entity.Property(e => e.Code)
                .HasColumnName("code");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            // Seed US state codes
            entity.HasData(GetStateCodeSeedData());
        });

        // Configure Store entity
        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Uuid)
                .HasColumnName("uuid");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.Address)
                .HasColumnName("address");

            entity.Property(e => e.City)
                .HasColumnName("city");

            entity.Property(e => e.StateCode)
                .HasColumnName("state_code");

            entity.Property(e => e.ZipCode)
                .HasColumnName("zip_code");

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("phone_number");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(e => e.State)
                .WithMany(s => s.Stores)
                .HasForeignKey(e => e.StateCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static StateCode[] GetStateCodeSeedData()
    {
        return new[]
        {
            new StateCode { Code = "AL", Name = "Alabama" },
            new StateCode { Code = "AK", Name = "Alaska" },
            new StateCode { Code = "AZ", Name = "Arizona" },
            new StateCode { Code = "AR", Name = "Arkansas" },
            new StateCode { Code = "CA", Name = "California" },
            new StateCode { Code = "CO", Name = "Colorado" },
            new StateCode { Code = "CT", Name = "Connecticut" },
            new StateCode { Code = "DE", Name = "Delaware" },
            new StateCode { Code = "FL", Name = "Florida" },
            new StateCode { Code = "GA", Name = "Georgia" },
            new StateCode { Code = "HI", Name = "Hawaii" },
            new StateCode { Code = "ID", Name = "Idaho" },
            new StateCode { Code = "IL", Name = "Illinois" },
            new StateCode { Code = "IN", Name = "Indiana" },
            new StateCode { Code = "IA", Name = "Iowa" },
            new StateCode { Code = "KS", Name = "Kansas" },
            new StateCode { Code = "KY", Name = "Kentucky" },
            new StateCode { Code = "LA", Name = "Louisiana" },
            new StateCode { Code = "ME", Name = "Maine" },
            new StateCode { Code = "MD", Name = "Maryland" },
            new StateCode { Code = "MA", Name = "Massachusetts" },
            new StateCode { Code = "MI", Name = "Michigan" },
            new StateCode { Code = "MN", Name = "Minnesota" },
            new StateCode { Code = "MS", Name = "Mississippi" },
            new StateCode { Code = "MO", Name = "Missouri" },
            new StateCode { Code = "MT", Name = "Montana" },
            new StateCode { Code = "NE", Name = "Nebraska" },
            new StateCode { Code = "NV", Name = "Nevada" },
            new StateCode { Code = "NH", Name = "New Hampshire" },
            new StateCode { Code = "NJ", Name = "New Jersey" },
            new StateCode { Code = "NM", Name = "New Mexico" },
            new StateCode { Code = "NY", Name = "New York" },
            new StateCode { Code = "NC", Name = "North Carolina" },
            new StateCode { Code = "ND", Name = "North Dakota" },
            new StateCode { Code = "OH", Name = "Ohio" },
            new StateCode { Code = "OK", Name = "Oklahoma" },
            new StateCode { Code = "OR", Name = "Oregon" },
            new StateCode { Code = "PA", Name = "Pennsylvania" },
            new StateCode { Code = "RI", Name = "Rhode Island" },
            new StateCode { Code = "SC", Name = "South Carolina" },
            new StateCode { Code = "SD", Name = "South Dakota" },
            new StateCode { Code = "TN", Name = "Tennessee" },
            new StateCode { Code = "TX", Name = "Texas" },
            new StateCode { Code = "UT", Name = "Utah" },
            new StateCode { Code = "VT", Name = "Vermont" },
            new StateCode { Code = "VA", Name = "Virginia" },
            new StateCode { Code = "WA", Name = "Washington" },
            new StateCode { Code = "WV", Name = "West Virginia" },
            new StateCode { Code = "WI", Name = "Wisconsin" },
            new StateCode { Code = "WY", Name = "Wyoming" },
            new StateCode { Code = "DC", Name = "District of Columbia" }
        };
    }
}