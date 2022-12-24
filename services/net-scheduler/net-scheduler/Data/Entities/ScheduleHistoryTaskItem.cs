namespace NetScheduler.Data.Entities;

public class ScheduleHistoryTaskItem : MongoBase
{
    public string ScheduleHistoryTaskId { get; set; }

    public string TaskId { get; set; }

    public string TaskName { get; set; }
}
