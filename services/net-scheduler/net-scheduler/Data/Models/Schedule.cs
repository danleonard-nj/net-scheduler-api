namespace NetScheduler.Data.Models;

using MongoDB.Bson.Serialization.Attributes;

public class Schedule : MongoBase
{
    [BsonElement("cron")]
    public string? Cron { get; set; }

    [BsonElement("include_seconds")]
    public bool IncludeSeconds { get; set; }

    [BsonElement("last_runtime")]
    public int? LastRuntime { get; set; }

    [BsonElement("links")]
    public IEnumerable<string>? Links { get; set; }

    [BsonElement("next_runtime")]
    public int? NextRuntime { get; set; }

    [BsonElement("queue")]
    public IEnumerable<int> Queue { get; set; } = Enumerable.Empty<int>();

    [BsonElement("schedule_id")]
    public string ScheduleId { get; set; } = null!;

    [BsonElement("schedule_name")]
    public string ScheduleName { get; set; } = null!;

    [BsonElement("is_active")]
    public bool? IsActive { get; set; }

    [BsonElement("updated_datetime")]
    public DateTime UpdateDateTime { get; set; }
}
