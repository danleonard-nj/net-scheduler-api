namespace NetScheduler.Data.Entities;

using MongoDB.Bson.Serialization.Attributes;

public class ScheduleTaskItem : MongoBase
{
    [BsonElement("endpoint")]
    public string? Endpoint { get; set; }

    [BsonElement("identity_client_id")]
    public string? IdentityClientId { get; set; }

    [BsonElement("method")]
    public string? Method { get; set; }

    [BsonElement("payload")]
    public string? Payload { get; set; }

    [BsonElement("task_id")]
    public string TaskId { get; set; } = null!;

    [BsonElement("task_name")]
    public string TaskName { get; set; } = null!;
}
