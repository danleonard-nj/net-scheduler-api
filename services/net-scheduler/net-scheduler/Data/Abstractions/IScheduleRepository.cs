namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;
using System.Linq.Expressions;

public interface IScheduleRepository
{
    Task<int> Delete(string id, CancellationToken token);

    Task<ScheduleItem> Get(string id, CancellationToken token);

    Task<IEnumerable<ScheduleItem>> GetAll(CancellationToken token);

    Task<ScheduleItem> Insert(ScheduleItem entity, CancellationToken token);

    Task<ScheduleItem> Replace(ScheduleItem entity, CancellationToken token);

    Task<IEnumerable<ScheduleItem>> Query(Expression<Func<ScheduleItem, bool>> query, CancellationToken token);
}
