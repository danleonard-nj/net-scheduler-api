namespace NetScheduler.Data.Entities;

using MongoDB.Bson.Serialization.Attributes;

public class ScheduleItem : MongoBase
{
    [BsonElement("schedule_id")]
    public string ScheduleId { get; set; } = null!;

    [BsonElement("schedule_name")]
    public string ScheduleName { get; set; } = null!;

    [BsonElement("schedule_type_id")]
    public string? ScheduleTypeId { get; set; }

    [BsonElement("cron")]
    public string Cron { get; set; } = null!;

    [BsonElement("include_seconds")]
    public bool IncludeSeconds { get; set; }

    [BsonElement("links")]
    public IEnumerable<string>? Links { get; set; }

    [BsonElement("next_runtime")]
    public int NextRuntime { get; set; }

    [BsonElement("last_runtime")]
    public int LastRuntime { get; set; }

    [BsonElement("queue")]
    public IEnumerable<int> Queue { get; set; } = Enumerable.Empty<int>();

    [BsonElement("is_active")]
    public bool? IsActive { get; set; }

    [BsonElement("updated_datetime")]
    public DateTime ModifiedDate { get; set; }

    [BsonElement("created_date")]
    public DateTime CreatedDate { get; set; }
}
