namespace NetScheduler.Services.Tasks.Abstractions;
using NetScheduler.Models.Identity;
using NetScheduler.Models.Tasks;

public interface ITaskService
{
    Task<ScheduleTaskModel> CreateTask(CreateTaskModel createTaskModel, CancellationToken token);

    Task DeleteTask(string taskId, CancellationToken token);

    Task<IEnumerable<ScheduleTaskModel>> ExecuteTasksAsync(
        IEnumerable<string> taskIds,
        string scheduleId,
        CancellationToken token);

    Task<ScheduleTaskModel> GetTask(string taskId, CancellationToken token);

    Task<IEnumerable<ScheduleTaskModel>> GetTasks(CancellationToken token);

    Task<ScheduleTaskModel> UpsertTask(ScheduleTaskModel scheduleTaskModel, CancellationToken token);

    Task<TokenModel> GetTokenAsync(string appId);
}
