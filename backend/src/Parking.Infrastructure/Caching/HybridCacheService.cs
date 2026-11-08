using Microsoft.Extensions.Caching.Hybrid;
using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Caching;

/// <summary>
/// HybridCache (L1 memory + L2 Redis) gives per-key single-flight execution, i.e. cache-stampede protection:
/// concurrent misses for the same key on an instance run the factory once.
/// </summary>
internal sealed class HybridCacheService(HybridCache cache) : ICacheService
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key, Func<CancellationToken, ValueTask<T>> factory, CacheEntrySettings settings, CancellationToken cancellationToken)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = settings.Expiration,
            LocalCacheExpiration = settings.LocalExpiration ?? settings.Expiration,
            Flags = settings.LocalExpiration is null ? HybridCacheEntryFlags.DisableLocalCache : HybridCacheEntryFlags.None,
        };
        return cache.GetOrCreateAsync(key, factory, options, cancellationToken: cancellationToken);
    }

    public ValueTask RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken) => cache.RemoveAsync(keys, cancellationToken);
}
