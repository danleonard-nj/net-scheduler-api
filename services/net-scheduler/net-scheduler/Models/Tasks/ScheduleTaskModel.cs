namespace NetScheduler.Models.Tasks;

using System.Text.Json.Serialization;

public class ScheduleTaskModel
{
    public string? Endpoint { get; set; }

    public string? IdentityClientId { get; set; }

    public string? Method { get; set; }

    public object? Payload { get; set; }

    public string? TaskId { get; set; }

    public string TaskName { get; set; } = null!;
}
