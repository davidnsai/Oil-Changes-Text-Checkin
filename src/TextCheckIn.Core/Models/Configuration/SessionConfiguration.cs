namespace TextCheckIn.Core.Models.Configuration;

public class SessionConfiguration
{
    public const string SectionName = "Session";

    public SessionStorageType StorageType { get; set; } = SessionStorageType.Database;

    public int DefaultExpirationMinutes { get; set; } = 30;

    public int CleanupIntervalMinutes { get; set; } = 60;

    public int MaxConcurrentSessions { get; set; } = 1000;

    public string? RedisConnectionString { get; set; }
}

public enum SessionStorageType
{
    Database,
    
    Redis
}
