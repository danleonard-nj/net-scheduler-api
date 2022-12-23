namespace NetScheduler.Services.History;

using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Models.Events;
using NetScheduler.Models.History;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.History.Abstractions;
using NetScheduler.Services.History.Extensions;

public class ScheduleHistoryService : IScheduleHistoryService
{
	private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
	private readonly IEventService _eventService;

	private readonly ILogger<ScheduleHistoryService> _logger;

	public ScheduleHistoryService(
		IScheduleHistoryRepository scheduleHistoryRepository,
		IEventService eventService,
		ILogger<ScheduleHistoryService> logger)
	{
		ArgumentNullException.ThrowIfNull(scheduleHistoryRepository, nameof(scheduleHistoryRepository));
		ArgumentNullException.ThrowIfNull(eventService, nameof(eventService));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_scheduleHistoryRepository = scheduleHistoryRepository;
		_eventService = eventService;
		_logger = logger;
	}

	public async Task DispatchCreateHistoryEntryEventAsync(
		ScheduleModel scheduleModel,
		IEnumerable<TaskModel> scheduleTaskModels)
	{
		
	}

	public async Task<ScheduleHistoryModel> CreateHistoryEntryAsync(
        ScheduleHistoryModel scheduleHistoryEntry,
        CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"{@Method}: {@ScheduleId}: {@ScheduleName}: {@TriggeredTimestamp}: Writing history entry",
			Caller.GetName(),
			scheduleHistoryEntry.ScheduleId,
			scheduleHistoryEntry.ScheduleName,
			scheduleHistoryEntry.TriggerDate);

		await _scheduleHistoryRepository.Insert(
			scheduleHistoryEntry.ToEntity(),
			cancellationToken);

		return scheduleHistoryEntry;
	}

	public async Task<IEnumerable<ScheduleHistoryModel>> GetHistoryByCreatedDateRangeAsync(
		int createdTimestampStart,
		int createdTimestampEnd = default,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation(
			"{@Method}: {@StartTimestamp}: {@EndTimestamp}: Get schedule history",
			Caller.GetName(),
			createdTimestampStart,
			createdTimestampEnd);

		var entities = await _scheduleHistoryRepository.GetScheduleHistoryByCreatedDateRangeAsync(
			createdTimestampStart,
			createdTimestampEnd,
			cancellationToken);

		var history = entities.Select(x => x.ToDomain());

		return history;
	}
}
