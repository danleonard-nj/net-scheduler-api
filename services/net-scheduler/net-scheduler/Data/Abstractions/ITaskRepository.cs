namespace NetScheduler.Data.Abstractions;
using NetScheduler.Data.Models;
using System.Linq.Expressions;

public interface ITaskRepository
{
    Task<int> Delete(string id, CancellationToken token);

    Task<ScheduleTask> Get(string id, CancellationToken token);

    Task<IEnumerable<ScheduleTask>> GetAll(CancellationToken token);

    Task<ScheduleTask> Insert(ScheduleTask entity, CancellationToken token);

    Task<IEnumerable<ScheduleTask>> Query(Expression<Func<ScheduleTask, bool>> query, CancellationToken token);

    Task<ScheduleTask> Update(ScheduleTask entity, CancellationToken token);
}
