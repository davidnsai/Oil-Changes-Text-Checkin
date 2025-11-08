namespace TextCheckIn.Shared.Models;

public class Power6Service
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required int EstimatedDurationMinutes { get; set; }

    public required decimal Price { get; set; }

    public required string Category { get; set; }

    public bool IsPopular { get; set; }
}

public static class Power6ServiceCatalog
{
    public const string TransmissionServiceId = "c6f1ef6a-8d9f-46ac-94e8-359c3aa11ffd";

    public const string CoolantServiceId = "3e354208-59d0-46e5-a6a0-f3cb47938c44";

    public const string IntakeCleaningServiceId = "4d816599-6a13-4aa4-959a-c8eb8168e71c";

    public const string PowerSteeringServiceId = "672b3a26-3f03-4db7-a7fb-317f4f60baf2";

    public const string GearBoxServiceId = "b54ad209-7e30-47c7-828d-1174dcebbf63";

    public const string CabinAirFilterServiceId = "f072fcb0-425f-4961-9894-cb6b7aec5133";

    public const string LubeOilFilterServiceId = "eceed116-245a-4be3-9e6e-16ec807c6241";

    public static readonly IReadOnlyList<Power6Service> Services = new List<Power6Service>
    {
        new Power6Service
        {
            Id = TransmissionServiceId,
            Name = "Transmission service",
            Description = "Complete transmission fluid replacement and system inspection to ensure smooth shifting and optimal performance.",
            EstimatedDurationMinutes = 45,
            Price = 189.99m,
            Category = "Drivetrain",
            IsPopular = true
        },
        new Power6Service
        {
            Id = CoolantServiceId,
            Name = "Coolant service",
            Description = "Coolant system flush and replacement to prevent overheating and maintain optimal engine temperature.",
            EstimatedDurationMinutes = 30,
            Price = 149.99m,
            Category = "Cooling System",
            IsPopular = true
        },
        new Power6Service
        {
            Id = IntakeCleaningServiceId,
            Name = "Intake system cleaning",
            Description = "Deep cleaning of intake valves and combustion chambers to restore engine performance and fuel efficiency.",
            EstimatedDurationMinutes = 60,
            Price = 229.99m,
            Category = "Engine Performance",
            IsPopular = false
        },
        new Power6Service
        {
            Id = PowerSteeringServiceId,
            Name = "Power steering fluid service",
            Description = "Power steering fluid replacement to ensure easy steering and prevent system damage.",
            EstimatedDurationMinutes = 20,
            Price = 89.99m,
            Category = "Steering System",
            IsPopular = false
        },
        new Power6Service
        {
            Id = GearBoxServiceId,
            Name = "Gear box service",
            Description = "Gearbox fluid replacement and inspection to maintain smooth gear changes and extend transmission life.",
            EstimatedDurationMinutes = 40,
            Price = 169.99m,
            Category = "Drivetrain",
            IsPopular = false
        },
        new Power6Service
        {
            Id = CabinAirFilterServiceId,
            Name = "Cabin Air Filter",
            Description = "Cabin air filter replacement to ensure clean air circulation and optimal HVAC performance.",
            EstimatedDurationMinutes = 15,
            Price = 49.99m,
            Category = "Interior Comfort",
            IsPopular = true
        },
        new Power6Service
        {
            Id = LubeOilFilterServiceId,
            Name = "Lube, Oil and Filter",
            Description = "Comprehensive oil change service including lubrication of chassis components and replacement of oil filter to ensure engine longevity.",
            EstimatedDurationMinutes = 25,
            Price = 99.99m,
            Category = "Braking System",
            IsPopular = true
        }
    };

    public static Power6Service? GetServiceById(string serviceId)
    {
        return Services.FirstOrDefault(s => s.Id == serviceId);
    }

    public static IEnumerable<string> GetAllServiceIds()
    {
        return Services.Select(s => s.Id);
    }

    public static bool IsValidPower6Service(string serviceId)
    {
        return Services.Any(s => s.Id == serviceId);
    }
}

