namespace NetScheduler.Data.Abstractions;
using NetScheduler.Data.Models;
using System.Linq.Expressions;

public interface IScheduleRepository
{
    Task<int> Delete(string id, CancellationToken token);

    Task<Schedule> Get(string id, CancellationToken token);

    Task<IEnumerable<Schedule>> GetAll(CancellationToken token);

    Task<Schedule> Insert(Schedule entity, CancellationToken token);

    Task<Schedule> Update(Schedule entity, CancellationToken token);

    Task<IEnumerable<Schedule>> Query(Expression<Func<Schedule, bool>> query, CancellationToken token);
}
