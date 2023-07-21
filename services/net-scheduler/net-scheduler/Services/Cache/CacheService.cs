namespace NetScheduler.Services.Cache;
using Microsoft.Extensions.Caching.Distributed;
using NetScheduler.Configuration;
using NetScheduler.Services.Cache.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IDistributedCache distributedCache,
        ILogger<CacheService> logger)
    {
        ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
        where T : class
    {
        var value = await _distributedCache.GetAsync(key);

        if (value == null)
        {
            _logger.LogInformation(
                "{@Method}: {@Key}: Cache miss",
                Caller.GetName(),
                key);

            return null;
        }

        _logger.LogInformation(
            "{@Method}: {@Key}: Cache hit",
            Caller.GetName(),
            key);

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        int ttlSeconds = 60)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
        };

        await _distributedCache.SetAsync(
            key,
            serialized,
            options);

        _logger.LogInformation(
            "{@Method}: {@Key}: Cache set",
            Caller.GetName(),
            key);
    }
}
