namespace NetScheduler.Models.Schedules;
public class ScheduleModel
{
    public string ScheduleId { get; set; } = null!;

    public string? ScheduleTypeId { get; set; }

    public string ScheduleName { get; set; } = null!;

    public string Cron { get; set; } = null!;

    public bool IncludeSeconds { get; set; } = false;

    public int LastRuntime { get; set; }

    public IEnumerable<string> Links { get; set; } = Enumerable.Empty<string>();

    public int NextRuntime { get; set; }

    public IEnumerable<int> Queue { get; set; } = Enumerable.Empty<int>();

    public bool? IsActive { get; set; }

    public DateTime ModifiedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}