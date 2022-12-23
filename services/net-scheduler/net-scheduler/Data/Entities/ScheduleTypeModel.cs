namespace NetScheduler.Data.Entities;
public class ScheduleTypeItem : MongoBase
{
    public string ScheduleTypeId { get; set; } = null!;

    public string ScheduleTypeName { get; set; } = null!;

    public string ScheduleTypeDescription { get; set; } = null!;

    public int CreatedDate { get; set; }

    public int ModifiedDate { get; set; }
}