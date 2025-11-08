using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    /// <summary>
    /// Service for Redis cache operations
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// Get value from Redis
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Set value in Redis with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Remove value from Redis
        /// </summary>
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if key exists
        /// </summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
