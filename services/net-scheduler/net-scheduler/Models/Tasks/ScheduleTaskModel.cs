namespace NetScheduler.Models.Tasks;

using System.Text.Json.Serialization;

public class ScheduleTaskModel
{
    public string TaskId { get; set; } = null!;

    public string TaskName { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string IdentityClientId { get; set; }

    public string Method { get; set; } = null!;

    public object? Payload { get; set; }
}
