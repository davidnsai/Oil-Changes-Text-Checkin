using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using TextCheckIn.Core.Models.Configuration;
using TextCheckIn.Core.Services.Interfaces;

namespace TextCheckIn.Core.Services;

/// <summary>
/// Redis cache service implementation
/// </summary>
public class RedisService : IRedisService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisService"/> class.
    /// </summary>
    /// <param name="sessionConfig">Session configuration containing Redis connection string</param>
    /// <param name="logger">Logger instance</param>
    public RedisService(
        IOptions<SessionConfiguration> sessionConfig,
        ILogger<RedisService> logger)
    {
        _logger = logger;
        
        var connectionString = sessionConfig.Value.RedisConnectionString 
            ?? throw new InvalidOperationException("Redis connection string is not configured");

        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
            _logger.LogInformation("Connected to Redis at {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis at {ConnectionString}", connectionString);
            throw;
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                _logger.LogDebug("Key not found in Redis: {Key}", key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            _logger.LogDebug("Retrieved value from Redis: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from Redis: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
            var success = await _database.StringSetAsync(key, jsonValue, expiration);
            
            if (success)
            {
                _logger.LogDebug("Stored value in Redis: {Key}, expires in {Expiration}", key, expiration);
            }
            else
            {
                _logger.LogWarning("Failed to store value in Redis: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing value in Redis: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _database.KeyDeleteAsync(key);
            
            if (success)
            {
                _logger.LogDebug("Removed key from Redis: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Key not found for removal in Redis: {Key}", key);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from Redis: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _database.KeyExistsAsync(key);
            _logger.LogDebug("Key exists check in Redis: {Key} = {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key existence in Redis: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Disposes the Redis connection
    /// </summary>
    public void Dispose()
    {
        _redis?.Dispose();
    }
}
