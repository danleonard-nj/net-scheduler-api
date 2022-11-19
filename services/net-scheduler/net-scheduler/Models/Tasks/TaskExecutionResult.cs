namespace NetScheduler.Models.Tasks;

public class TaskExecutionResult
{
    public TaskExecutionResult(string taskId)
    {
        TaskId = taskId;
    }

    public string TaskId { get; set; } = null!;
    public object? ExecutionResult { get; set; }
}