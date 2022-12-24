namespace NetScheduler.Models.Schedules;

public class ScheduleTypeModel
{
    public string ScheduleTypeId { get; set; } = null!;

    public string ScheduleType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CreatedDate { get; set; }

    public int ModifiedDate { get; set; }
}