namespace NetScheduler.Services.Extensions;
public static class DateTimeExtensions
{
    public static DateTimeOffset ToLocalDateTime(this int timestamp)
    {
        var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");

        return TimeZoneInfo.ConvertTimeFromUtc(offset.DateTime, timeZone);
    }
}
