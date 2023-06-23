namespace NetScheduler.Data.Entities;
public class ScheduleItem : MongoBase
{
    public string ScheduleId { get; set; } = null!;

    public string ScheduleName { get; set; } = null!;

    public string? ScheduleType { get; set; }

    public string Cron { get; set; } = null!;

    public bool IncludeSeconds { get; set; }

    public IEnumerable<string>? Links { get; set; }

    public int NextRuntime { get; set; }

    public int LastRuntime { get; set; }

    public IEnumerable<int> Queue { get; set; } = Enumerable.Empty<int>();

    public bool? IsActive { get; set; }

    public DateTime ModifiedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
