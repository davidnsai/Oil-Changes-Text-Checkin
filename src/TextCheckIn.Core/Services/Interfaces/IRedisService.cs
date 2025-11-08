using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    public interface IRedisService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
