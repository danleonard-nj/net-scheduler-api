namespace NetScheduler.Services.History.Abstractions;

using NetScheduler.Models.History;

public interface IScheduleHistoryService
{
    Task<ScheduleHistoryModel> CreateHistoryEntryAsync(
        ScheduleHistoryModel scheduleHistoryEntry,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ScheduleHistoryModel>> GetHistoryByCreatedDateRangeAsync(
        int createdTimestampStart,
        int createdTimestampEnd = 0,
        CancellationToken cancellationToken = default);
}
