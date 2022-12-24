namespace NetScheduler.Data.Entities;

public class ScheduleHistoryItem : MongoBase
{
    public string ScheduleHistoryId { get; set; }

    public string ScheduleId { get; set; }

    public string ScheduleName { get; set; }

    public IEnumerable<ScheduleHistoryTaskItem> Tasks { get; set; }

    public int TriggerDate { get; set; }

    public int CreatedDate { get; set; }
}
