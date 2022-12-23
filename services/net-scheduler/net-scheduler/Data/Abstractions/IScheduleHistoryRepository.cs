namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;

public interface IScheduleHistoryRepository : IMongoRepository<ScheduleHistoryItem>
{
    Task<IEnumerable<ScheduleHistoryItem>> GetScheduleHistoryByCreatedDateRangeAsync(
        int startCreatedDate,
        int endCreatedDate,
        CancellationToken token);
}
