namespace TextCheckIn.Core.Models.Configuration;

/// <summary>
/// Configuration settings for session management
/// </summary>
public class SessionConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Session";

    /// <summary>
    /// Storage type for sessions (Database or Redis)
    /// </summary>
    public SessionStorageType StorageType { get; set; } = SessionStorageType.Database;

    /// <summary>
    /// Default session expiration in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Cleanup interval in minutes for expired sessions
    /// </summary>
    public int CleanupIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum concurrent sessions allowed
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 1000;

    /// <summary>
    /// Redis connection string (used when StorageType is Redis)
    /// </summary>
    public string? RedisConnectionString { get; set; }
}

/// <summary>
/// Session storage type enumeration
/// </summary>
public enum SessionStorageType
{
    /// <summary>
    /// Store sessions in the database
    /// </summary>
    Database,
    
    /// <summary>
    /// Store sessions in Redis cache
    /// </summary>
    Redis
}
