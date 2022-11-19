namespace NetScheduler.Models.Cache;

using Microsoft.Extensions.Caching.Distributed;

public static class CacheOptions
{
    public static DistributedCacheEntryOptions ExpirationMinutes(int minutes)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes),
        };
    }

    public static DistributedCacheEntryOptions ExpirationHours(int hours)
    {
        return ExpirationMinutes(hours * 60);
    }
}
