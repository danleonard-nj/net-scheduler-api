namespace NetScheduler.Services.Extensions;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public static class CacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache distributedCache, string cacheKey, CancellationToken token)
        where T : class
    {
        var value = await distributedCache.GetStringAsync(
            cacheKey,
            token);

        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value);
    }

    public static async Task SetAsync<T>(
        this IDistributedCache distributedCache,
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken token)
    {
        await distributedCache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            options,
            token);
    }
}