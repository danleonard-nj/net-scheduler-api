namespace NetScheduler.Services.Tasks.Abstractions;
using NetScheduler.Models.Identity;
using NetScheduler.Models.Tasks;

public interface ITaskService
{
    Task<TaskModel> CreateTask(CreateTaskModel createTaskModel, CancellationToken token);

    Task DeleteTask(string taskId, CancellationToken token);

    Task<IEnumerable<(TaskModel task, string invocationId)>> ExecuteTasksAsync(
        IEnumerable<string> taskIds,
        string scheduleId,
        CancellationToken token);

    Task<TaskModel> GetTask(string taskId, CancellationToken token);

    Task<IEnumerable<TaskModel>> GetTasks(CancellationToken token);

    Task<TaskModel> UpsertTask(TaskModel scheduleTaskModel, CancellationToken token);

    Task<TokenModel> GetTokenAsync(string appId);
}
