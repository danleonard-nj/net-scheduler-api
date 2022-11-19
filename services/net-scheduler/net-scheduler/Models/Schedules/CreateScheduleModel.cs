namespace NetScheduler.Models.Schedules;

public class CreateScheduleModel
{
    public string ScheduleName { get; set; } = null!;

    public string Cron { get; set; } = null!;

    public bool IncludeSeconds { get; set; } = false;
}
