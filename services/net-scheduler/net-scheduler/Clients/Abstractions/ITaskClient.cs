namespace NetScheduler.Clients.Abstractions;
using NetScheduler.Models.Tasks;

public interface ITaskClient
{
    Task<object?> ExecuteTask(TaskModel task, CancellationToken token);
}
