namespace NetScheduler.Services.Schedules;

using Microsoft.Extensions.Caching.Distributed;
using NetScheduler.Clients.Abstractions;
using NetScheduler.Clients.Constants;
using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Models.Cache;
using NetScheduler.Models.Features;
using NetScheduler.Models.History;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Cache.Abstractions;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.History.Extensions;
using NetScheduler.Services.Schedules.Abstractions;
using NetScheduler.Services.Schedules.Exceptions;
using NetScheduler.Services.Schedules.Extensions;
using NetScheduler.Services.Schedules.Helpers;
using NetScheduler.Services.Tasks.Abstractions;
using System.Threading.Tasks;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ITaskService _taskService;
    private readonly IEventService _eventService;
    private readonly IFeatureClient _featureClient;

    private readonly IDistributedCache _distributedCache;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        ITaskService taskService,
        IFeatureClient featureClient,
        IEventService eventService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ILogger<ScheduleService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(scheduleRepository, nameof(scheduleRepository));
        ArgumentNullException.ThrowIfNull(taskService, nameof(taskService));
        ArgumentNullException.ThrowIfNull(featureClient, nameof(featureClient));
        ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));
        ArgumentNullException.ThrowIfNull(eventService, nameof(eventService));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));

        _scheduleRepository = scheduleRepository;
        _distributedCache = distributedCache;
        _featureClient = featureClient;
        _taskService = taskService;
        _eventService = eventService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskExecutionResult>> Poll(CancellationToken token)
    {
        PollState.SetPolling(false);

        var startTimestamp = DateTimeOffset.UtcNow;

        var evaluatedFeatures = await GetPollFeatureFlagDetailsAsync(
            token);
        
        // TODO: Reference result model props directly
        var isEnabled = evaluatedFeatures.IsSchedulerEnabled;
        var calcDisplayEnabled = evaluatedFeatures.IsCalculationDetailEnabled;
        var historyEnabled = evaluatedFeatures.IsHistoryEnabled;
        var forceCalculate = evaluatedFeatures.IsForceCalculateTimestampsEnabled;

        _logger.LogInformation(
              "{@Method}: {@IsSchedulerEnabled}: {@IsCalculationDisplayEnabled}: {@IsScheduleHistoryEnabled}: Feature flags",
              Caller.GetName(),
              isEnabled,
              calcDisplayEnabled,
              historyEnabled);

        if (!isEnabled)
        {
            _logger.LogInformation(
               "{@Method}: Scheduler is disabled",
               Caller.GetName());

            return Enumerable.Empty<TaskExecutionResult>();
        }

        if (forceCalculate)
        {
            _logger.LogInformation(
              "{@Method}: Force updating schedule timestamps",
              Caller.GetName());

            await ForceUpdateScheduleTimestamps(token);
        }

        // Get all currently active schedules
        var activeSchedules = await GetActiveSchedulesAsync(
            token);

        var executionQueue = new List<TriggeredScheduleModel>();

        _logger.LogInformation(
           "{@Method}: {@ScheduleCount}: Active schedules to process",
           Caller.GetName(),
           activeSchedules.Count());

        foreach (var schedule in activeSchedules)
        {
            try
            {
                var (isTriggered, updatedSchedule) = await EvaluateScheduleTriggerAsync(
                    schedule,
                    token);

                if (calcDisplayEnabled)
                {
                    _logger.LogInformation(
                        "{@Method}: {@ScheduleId}: {@ScheduleName}: {@IsTriggered}: {@TimeRemaining}: Schedule trigger state",
                        Caller.GetName(),
                        schedule.ScheduleId,
                        schedule.ScheduleName,
                        isTriggered,
                        schedule.GetTimeRemaining());
                }

                // If the schedule is triggered add to the
                // execution queue
                if (isTriggered)
                {
                    _logger.LogInformation(
                        "{@Method}: {@ScheduleId}: {@ScheduleName}: {@IsTriggered}: {@TimeRemaining}: Adding schedule to execution queue",
                        Caller.GetName(),
                        schedule.ScheduleId,
                        schedule.ScheduleName,
                        isTriggered,
                        schedule.GetTimeRemaining());

                    executionQueue.Add(schedule.ToTriggeredScheduleModel());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{@Method}: {@ScheduleId}: {@ScheduleName}: Failed to evaluate schedule: {@Message}",
                    Caller.GetName(),
                    schedule.ScheduleId,
                    schedule.ScheduleName,
                    ex.Message);
            }

        }

        // Run triggered schedule tasks
        if (executionQueue.Any())
        {
            _logger.LogInformation(
               "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedules queued for execution",
               Caller.GetName(),
               executionQueue?.Select(x => x.Schedule.ScheduleId),
               executionQueue?.Select(x => x.Schedule.ScheduleName));

            await RunTriggeredSchedulesAsync(
                executionQueue,
                historyEnabled,
                token);
        }

        var elapsed = DateTimeOffset.UtcNow - startTimestamp;

        // Log schedule processing time
        _logger.LogInformation(
            "{@Method}: Schedule poll cycle completed in {@MillisecondsElapsed} ms",
            Caller.GetName(),
            elapsed.TotalMilliseconds);

        _logger.LogInformation(
            "{Method}: Cached CRON expressions: {@Count}",
            Caller.GetName(),
            CronExpressionParser.GetParsedExpressionCache().Count);

        PollState.SetPolling(false);

        return Enumerable.Empty<TaskExecutionResult>();
    }

    public async Task<ScheduleModel> GetSchedule(
        string scheduleId,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(scheduleId))
        {
            throw new ArgumentNullException(nameof(scheduleId));
        }

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: Get schedule",
            Caller.GetName(),
            scheduleId);

        var schedule = await _scheduleRepository.Get(
            scheduleId,
            token);

        if (schedule == null)
        {
            throw new ScheduleNotFoundException($"No schedule with the ID '{scheduleId}' exists");
        }

        _logger.LogInformation(
            "{@Method}: {@ScheduleName}: {@ScheduleId}: Fetched schedule",
            Caller.GetName(),
            schedule?.ScheduleName,
            scheduleId);

        return schedule.ToDomain();
    }

    public async Task<ScheduleModel> CreateSchedule(
        CreateScheduleModel createScheduleModel,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(createScheduleModel, nameof(createScheduleModel));

        _logger.LogInformation(
            "{@Method}: {@CreateScheduleModel}: Create schedule",
            Caller.GetName(),
            createScheduleModel);

        if (string.IsNullOrWhiteSpace(createScheduleModel.ScheduleName))
        {
            throw new InvalidScheduleException("Schedule name cannot be null");
        }

        // TODO: Move to extension
        var schedule = new ScheduleModel
        {
            ScheduleId = Guid.NewGuid().ToString(),
            ScheduleName = createScheduleModel.ScheduleName,
            IsActive = true,
            Links = Enumerable.Empty<string>(),
            Queue = Enumerable.Empty<int>(),
            Cron = createScheduleModel.Cron,
            IncludeSeconds = createScheduleModel.IncludeSeconds,
            LastRuntime = default,
            NextRuntime = default,
            CreatedDate = DateTime.UtcNow,
            ScheduleType = ScheduleType.User.ToString()
        };

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedule created",
            Caller.GetName(),
            schedule?.ScheduleId,
            schedule?.ScheduleName);

        await _scheduleRepository.Insert(
            schedule.ToEntity(),
            token);

        return schedule;
    }

    public async Task<IEnumerable<ScheduleModel>> GetSchedules(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
           "{@Method}: Fetching schedule list",
           Caller.GetName());

        var entities = await _scheduleRepository.GetAll(
            cancellationToken);

        var schedules = entities
            .Select(x => x.ToDomain())
            .Where(x => x.ScheduleType == ScheduleType.User.ToString());

        _logger.LogInformation(
           "{@Method}: {@Count}: Schedules fetched",
           Caller.GetName(),
           schedules?.Count());

        return schedules;
    }

    public async Task<ScheduleModel> UpdateSchedule(
        ScheduleModel scheduleModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scheduleModel, nameof(scheduleModel));

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: Upsert schedule",
            Caller.GetName(),
            scheduleModel.ScheduleId,
            scheduleModel.ScheduleName);

        // Validate schedule
        if (string.IsNullOrEmpty(scheduleModel.ScheduleId))
        {
            _logger.LogError(
                "{@Method}: {@ScheduleId}: {@ScheduleName}: Invalid schedule ID",
                Caller.GetName(),
                scheduleModel.ScheduleId,
                scheduleModel.ScheduleName);

            throw new ScheduleNotFoundException(
                $"Schedule ID '{scheduleModel.ScheduleId}' is not valid");
        }

        if (string.IsNullOrWhiteSpace(scheduleModel.ScheduleName))
        {
            throw new InvalidScheduleException("Schedule name cannot be null");
        }

        if (string.IsNullOrWhiteSpace(scheduleModel.Cron))
        {
            throw new InvalidScheduleException("Schedule CRON expression cannot be null");
        }

        var existingEntity = await _scheduleRepository.Get(
            scheduleModel.ScheduleId,
            cancellationToken);

        if (existingEntity == null)
        {
            _logger.LogError(
                "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedule not found",
                Caller.GetName(),
                scheduleModel.ScheduleId,
                scheduleModel.ScheduleName);

            throw new ScheduleNotFoundException(
                $"No schedule with the ID '{scheduleModel.ScheduleId}' exists");
        }

        var existingSchedule = existingEntity.ToDomain();

        //// TODO: Move to extension
        //entity.ScheduleName = scheduleModel.ScheduleName;
        //entity.IsActive = scheduleModel.IsActive;

        //entity.Cron = scheduleModel.Cron;
        //entity.IncludeSeconds = scheduleModel.IncludeSeconds;
        //entity.Links = scheduleModel.Links;

        //// Clear timestamps to be recalculated
        //entity.NextRuntime = default;
        //entity.Queue = Enumerable.Empty<int>();

        //entity.LastRuntime = scheduleModel.LastRuntime;
        //entity.ModifiedDate = DateTime.Now;

        var updatedSchedule = existingSchedule.UpdateScheduleDetails(
            scheduleModel);

        var updatedEntity = await _scheduleRepository.Replace(
            updatedSchedule.ToEntity(),
            cancellationToken);

        _logger.LogInformation(
            "{@Method}: {@Schedule}: Updated schedule",
            Caller.GetName(),
            updatedEntity);

        return updatedSchedule;
    }

    public async Task DeleteSchedule(
        string scheduleId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scheduleId))
        {
            throw new ArgumentNullException(nameof(scheduleId));
        }

        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: Delete schedule",
           Caller.GetName(),
           scheduleId);

        var entity = await _scheduleRepository.Get(
            scheduleId,
            cancellationToken);

        if (entity == null)
        {
            throw new ScheduleNotFoundException(
                $"No schedule with the ID '{scheduleId}' exists");
        }

        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedule name to delete",
           Caller.GetName(),
           scheduleId,
           entity.ScheduleName);

        var result = await _scheduleRepository.Delete(
            scheduleId,
            cancellationToken);

        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: {@ScheduleName}: {@IsDeleted}: Delete results",
           Caller.GetName(),
           scheduleId,
           entity.ScheduleName,
           result > 0);
    }

    public async Task RunSchedule(
        string scheduleId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scheduleId))
        {
            throw new ArgumentNullException(nameof(scheduleId));
        }

        var entity = await _scheduleRepository.Get(
            scheduleId,
            cancellationToken);

        if (entity == null)
        {
            throw new ScheduleNotFoundException($"No schedule with the ID '{scheduleId}' exists");
        }

        var schedule = entity.ToDomain();

        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: {@ScheduleName}: {@Links}: Run schedule",
           Caller.GetName(),
           scheduleId,
           schedule.ScheduleName,
           schedule.Links);

        if (entity.Links == null || !entity.Links.Any())
        {
            _logger.LogInformation(
                "{@Method}: {@ScheduleId}: No linked tasks to execute",
                Caller.GetName(),
                scheduleId);

            return;
        }

        var isHistoryEnabled = await _featureClient.EvaluateFeature(
            Feature.SchedulerExecutionHistory,
            cancellationToken);

        var manualTriggeredSchedule = schedule.ToTriggeredScheduleModel(
            true);

        await RunTriggeredSchedulesAsync(
            new[] { manualTriggeredSchedule },
            isHistoryEnabled,
            cancellationToken);
    }

    private async Task<(bool isTriggered, ScheduleModel updatedSchedule)> EvaluateScheduleTriggerAsync(
        ScheduleModel schedule,
        CancellationToken token = default)
    {
        // Set initial values for new schedules
        if (schedule.NextRuntime == default)
        {
            _logger.LogInformation(
                "{@Method}: {@ScheduleName}: Initial update for schedule trigger values",
                Caller.GetName(),
                schedule.ScheduleName);

            await InitializeScheduleAsync(
                schedule,
                token);
        }

        var isInvoked = schedule.GetScheduleInvocationState();

        if (isInvoked)
        {
            _logger.LogInformation(
                "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedule triggered",
                Caller.GetName(),
                schedule.ScheduleId,
                schedule.ScheduleName);

            var updatedSchedule = UpdateScheduleRuntimes(
                schedule);

            await _scheduleRepository.Replace(
                updatedSchedule.ToEntity(),
                token);

            return (true, updatedSchedule);
        }

        return (false, schedule);
    }

    private async Task ForceUpdateScheduleTimestamps(
        CancellationToken token)
    {
        var scheduleEntities = await _scheduleRepository
            .GetAll(token);

        var allSchedulees = scheduleEntities
            .Select(x => x.ToDomain());

        foreach (var forceUpdateSchedule in allSchedulees)
        {
            var updaatedSchedule = UpdateScheduleRuntimes(forceUpdateSchedule);

            await _scheduleRepository.Replace(
                updaatedSchedule.ToEntity(),
                token);
        }
    }

    private ScheduleModel UpdateScheduleRuntimes(
        ScheduleModel schedule)
    {
        // Update the last schedule runtime
        schedule.UpdateLastRuntime();

        // Parse the schedule cron expression
        var expression = schedule.GetCronExpression();

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Phoenix");

        var dateQueue = expression
            .GetOccurrences(
                DateTime.UtcNow,
                DateTime.UtcNow + TimeSpan.FromDays(7),
                timeZone);

        var queue = dateQueue.Select(x => (int)new DateTimeOffset(x)
            .ToUnixTimeSeconds());

        schedule.Queue = queue.Take(5);

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: {@RuntimeQueue}: Schedule runtimes",
            Caller.GetName(),
            schedule.ScheduleId,
            schedule.ScheduleName,
            schedule.Queue);

        schedule.NextRuntime = queue.FirstOrDefault();

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: {@NextRuntime}: {@TimeRemaining}: Next schedule runtime",
            Caller.GetName(),
            schedule.ScheduleId,
            schedule.ScheduleName,
            schedule.NextRuntime,
            schedule.GetTimeRemaining());

        schedule.ModifiedDate = DateTime.UtcNow;
        return schedule;
    }

    private async Task<IEnumerable<ScheduleModel>> RunTriggeredSchedulesAsync(
        IEnumerable<TriggeredScheduleModel> triggeredSchedules,
        bool isHistoryEnabled = true,
        CancellationToken token = default)
    {
        var allTriggeredSchedules = triggeredSchedules
            .Select(x => x.Schedule);

        _logger.LogInformation(
            "{@Method}: {@QueueLength}: Processing task execution queue",
            Caller.GetName(),
            triggeredSchedules.Count());

        // Only run schedules w/ linked tasks
        var schedules = triggeredSchedules
            .Where(x => x.Schedule.Links.Any());

        var links = schedules
            .SelectMany(x => x.Schedule.Links)
            .Distinct();

        // If no triggered schedules have linked tasks
        if (!links.Any())
        {
            return allTriggeredSchedules;
        }

        _logger.LogInformation(
            "{@Method}: {@Links}: Triggered schedule linked tasks",
            Caller.GetName(),
            links);

        var tasks = await _taskService.ExecuteTasksAsync(
            links,
            token);

        // Purposely not awaiting this call to avoid blocking
        // TODO: Configure await necessary here?
        HandleSchedulerHistoryAsync(
            schedules,
            tasks);

        return allTriggeredSchedules;
    }

    private async Task<IEnumerable<ScheduleHistoryModel>> HandleSchedulerHistoryAsync(
        IEnumerable<TriggeredScheduleModel> schedules,
        IEnumerable<(TaskModel task, string invocationId)> tasks,
        CancellationToken cancellationToken = default)
    {
        var history = new List<ScheduleHistoryModel>();

        var taskLookup = tasks.ToDictionary(x => x.task.TaskId);

        // Build the schedule invocation history
        foreach (var schedule in schedules)
        {
            // Lookup the tasks and add them to the history
            var invocationTasks = new List<ScheduleTaskHistoryModel>();

            foreach (var taskId in schedule.Schedule.Links)
            {
                // Get the task and the invocations from run results
                if (taskLookup.TryGetValue(taskId, out var result))
                {
                    var (task, invocationId) = result;

                    invocationTasks.Add(new ScheduleTaskHistoryModel
                    {
                        InvocationId = invocationId,
                        ScheduleHistoryTaskId = Guid.NewGuid().ToString(),
                        TaskId = task.TaskId,
                        TaskName = task.TaskName
                    });
                }
            }

            var invocation = new ScheduleHistoryModel
            {
                ScheduleHistoryId = Guid.NewGuid().ToString(),
                ScheduleId = schedule.Schedule.ScheduleId,
                TriggerDate = schedule.Schedule.LastRuntime,
                IsManualTrigger = schedule.IsManual,
                ScheduleName = schedule.Schedule.ScheduleName,
                Tasks = invocationTasks,
                CreatedDate = GetTimestamp()
            };

            history.Add(invocation);
        }

        await _eventService
            .DispatchScheduleHistoryEventAsync(history,cancellationToken)
            .ConfigureAwait(false);

        return history;
    }

    private async Task<IEnumerable<ScheduleModel>> GetActiveSchedulesAsync(
        CancellationToken token)
    {
        var schedules = await _scheduleRepository.GetAll(
                token);

        var activeSchedules = schedules
            .Where(x => x.IsActive ?? true)
            .ToList();

        return activeSchedules.Select(x => x.ToDomain());
    }

    private static int GetTimestamp()
    {
        return (int)DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    private async Task<FeatureEvaluationResult> GetPollFeatureFlagDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        var evaluatedFeatures = await _cacheService.GetAsync<FeatureEvaluationResult>(
            CacheKey.FeatureFlags());

        if (evaluatedFeatures != null)
        {
            return evaluatedFeatures;
        }

        // Fetch flags in parallel
        var results = await Task.WhenAll(
            _featureClient.EvaluateFeature(
                Feature.NetScheduler,
                cancellationToken),
            _featureClient.EvaluateFeature(
                Feature.SchedulerConsoleDisplayCalculationDetails,
                cancellationToken),
            _featureClient.EvaluateFeature(
                Feature.SchedulerExecutionHistory, 
                cancellationToken),
            _featureClient.EvaluateFeature(
                Feature.NetSchedulerCalculateTimestamps,
                cancellationToken));

        evaluatedFeatures = new FeatureEvaluationResult(
            results[0],
            results[1],
            results[2],
            results[3]);

        await _cacheService.SetAsync(
            CacheKey.FeatureFlags(),
            evaluatedFeatures,
            30);

        return evaluatedFeatures;
    }

    private async Task InitializeScheduleAsync(
        ScheduleModel schedule,
        CancellationToken token)
    {
        _logger.LogInformation(
            "{@Method}: {@ScheduleName}: Initial update for schedule trigger values",
            Caller.GetName(),
            schedule.ScheduleName);

        UpdateScheduleRuntimes(schedule);

        await _scheduleRepository.Replace(
            schedule.ToEntity(),
            token);
    }
}
