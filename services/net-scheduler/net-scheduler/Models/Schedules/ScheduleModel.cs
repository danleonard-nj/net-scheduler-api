namespace NetScheduler.Models.Schedules;
public class ScheduleModel
{
    public string? ScheduleId { get; set; }

    public string? ScheduleTypeId { get; set; }

    public string? ScheduleName { get; set; }

    public string? Cron { get; set; }

    public bool IncludeSeconds { get; set; } = false;

    public DateTimeOffset? LastRuntime { get; set; }

    public IEnumerable<string> Links { get; set; } = Enumerable.Empty<string>();

    public DateTimeOffset? NextRuntime { get; set; }

    public IEnumerable<DateTimeOffset> Queue { get; set; } = Enumerable.Empty<DateTimeOffset>();

    public bool? IsActive { get; set; }

    public DateTime UpdatedDateTime { get; set; }
}
