namespace NetScheduler.Services.Events.Abstractions;

using NetScheduler.Models.Events;
using NetScheduler.Models.History;
using System.Threading.Tasks;

public interface IEventService
{
    string ApplicationBaseUrl { get; }

    Task DispatchEventAsync(
        ApiEvent apiEvent,
        string identityClientId,
        CancellationToken cancellationToken = default);

    Task DispatchEventsAsync(
        IEnumerable<ApiEvent> apiEvents,
        string identityClientId,
        CancellationToken cancellationToken = default);

    Task DispatchScheduleHistoryEventAsync(
        ScheduleHistoryModel scheduleHistoryModel,
        CancellationToken cancellationToken = default);
}