namespace NetScheduler.Services.Schedules.Helpers;
using Cronos;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;

public static class CronExpressionParser
{
    private static readonly ConcurrentDictionary<string, CronExpression> CronCache = new();

    public static bool TryParse(
        string cron,
        bool includeSeconds,
        out CronExpression expression)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            throw new ArgumentNullException(nameof(cron));
        }

        // Generate a cache key for the expression based
        // on the cron string and the parser parameters
        var key = GetCronExpressionCacheKey(
            cron,
            includeSeconds);

        if (CronCache.TryGetValue(key, out expression))
        {
            return true;
        }

        try
        {
            expression = CronExpression.Parse(cron);

            // Cache the parsed expression
            CronCache.TryAdd(key, expression);

            return true;
        }
        catch (Exception)
        {
            expression = default;
            return false;
        }
    }

    public static IReadOnlyDictionary<string, CronExpression> GetParsedExpressionCache() => CronCache;

    private static string GetCronExpressionCacheKey(string crontab, bool includeSeconds) => $"{crontab}-{includeSeconds}";
}