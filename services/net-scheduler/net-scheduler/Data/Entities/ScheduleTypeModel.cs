namespace NetScheduler.Data.Entities;

using MongoDB.Bson.Serialization.Attributes;

public class ScheduleTypeItem : MongoBase
{
    [BsonElement("schedule_type_id")]
    public string ScheduleTypeId { get; set; } = null!;

    [BsonElement("schedule_type")]
    public string ScheduleTypeName { get; set; } = null!;

    [BsonElement("schedule_type_description")]
    public string ScheduleTypeDescription { get; set; } = null!;

    [BsonElement("created_date")]
    public int CreatedDate { get; set; }

    [BsonElement("modified_date")]
    public int ModifiedDate { get; set; }
}