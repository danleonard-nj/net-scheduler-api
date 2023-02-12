namespace NetScheduler.Models.Schedules;
public class TriggeredScheduleModel
{
    public ScheduleModel Schedule { get; set; }

    public bool IsManual { get; set; }
}
