namespace NetScheduler.Models.Tasks;

using System.Text.Json.Serialization;

public class CreateTaskModel
{
    public string TaskName { get; set; } = null!;

    public string? IdentityClientId { get; set; }

    public string? Endpoint { get; set; }

    public string? Method { get; set; }

    public object? Payload { get; set; }
}