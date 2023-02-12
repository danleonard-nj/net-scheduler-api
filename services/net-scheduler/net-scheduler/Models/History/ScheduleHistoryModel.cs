namespace NetScheduler.Models.History;

public class ScheduleHistoryModel
{
    public string ScheduleHistoryId { get; set; } = null!;

    public string ScheduleId { get; set; } = null!;

    public string ScheduleName { get; set; } = null!;

    public IEnumerable<ScheduleTaskHistoryModel> Tasks { get; set; }

    public bool IsManualTrigger { get; set; }

    public int TriggerDate { get; set; }

    public int CreatedDate { get; set; }
}
