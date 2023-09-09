using NetScheduler.Data.Entities;

namespace NetScheduler.Models.Tasks;
public class ScheduleTypeItem : MongoBase
{
    public string ScheduleTypeId { get; set; } = null!;

    public string ScheduleTypeName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CreatedDate { get; set; }

    public int ModifiedDate { get; set; }
}