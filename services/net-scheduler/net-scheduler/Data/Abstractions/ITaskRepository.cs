namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;
using System.Linq.Expressions;

public interface ITaskRepository : IMongoRepository<ScheduleTaskItem>
{
    Task<IEnumerable<ScheduleTaskItem>> GetTasksAsync(
        IEnumerable<string> taskIds,
        CancellationToken cancellationToken);

    Task<IEnumerable<ScheduleTaskItem>> Query(
        Expression<Func<ScheduleTaskItem, bool>> query,
        CancellationToken token);
}