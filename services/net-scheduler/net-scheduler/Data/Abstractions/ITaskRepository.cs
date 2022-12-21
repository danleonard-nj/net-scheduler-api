namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;
using System.Linq.Expressions;

public interface ITaskRepository
{
    Task<int> Delete(string id, CancellationToken token);

    Task<ScheduleTaskItem> Get(string id, CancellationToken token);

    Task<IEnumerable<ScheduleTaskItem>> GetAll(CancellationToken token);

    Task<ScheduleTaskItem> Insert(ScheduleTaskItem entity, CancellationToken token);

    Task<IEnumerable<ScheduleTaskItem>> Query(Expression<Func<ScheduleTaskItem, bool>> query, CancellationToken token);

    Task<ScheduleTaskItem> Replace(ScheduleTaskItem entity, CancellationToken token);
}
