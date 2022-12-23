namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;
using System.Linq.Expressions;

public interface ITaskRepository : IMongoRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetTasksAsync(
        IEnumerable<string> taskIds,
        CancellationToken cancellationToken);

    Task<IEnumerable<TaskItem>> Query(
        Expression<Func<TaskItem, bool>> query,
        CancellationToken token);
}