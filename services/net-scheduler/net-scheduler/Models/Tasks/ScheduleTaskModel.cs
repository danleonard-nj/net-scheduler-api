﻿namespace NetScheduler.Models.Tasks;

using System.Text.Json.Serialization;

public class TaskModel
{
    public string TaskId { get; set; } = null!;

    public string? CategoryId { get; set; }

    public string TaskName { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string IdentityClientId { get; set; }

    public string Method { get; set; } = null!;

    public object? Payload { get; set; }
}
