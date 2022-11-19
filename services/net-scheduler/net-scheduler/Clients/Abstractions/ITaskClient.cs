namespace NetScheduler.Clients.Abstractions;
using NetScheduler.Models.Tasks;

public interface ITaskClient
{
    Task<object?> ExecuteTask(ScheduleTaskModel task, CancellationToken token);
}
