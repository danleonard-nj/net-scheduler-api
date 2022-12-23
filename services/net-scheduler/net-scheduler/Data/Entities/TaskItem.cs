namespace NetScheduler.Data.Entities;

using MongoDB.Bson.Serialization.Attributes;

public class TaskItem : MongoBase
{
    public string TaskId { get; set; } = null!;

    public string TaskName { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string? IdentityClientId { get; set; }

    public string? Method { get; set; }

    public string? Payload { get; set; }
}
