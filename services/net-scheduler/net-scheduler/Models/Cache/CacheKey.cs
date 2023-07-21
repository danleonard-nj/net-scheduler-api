namespace NetScheduler.Models.Cache;

public static class CacheKey
{
    public static string ActiveSchedules() => "net-scheduler-active-schedules";

    public static string FeatureFlags() => "net-scheduler-feature-flags";
}
