namespace NetScheduler.Services.Schedules;

using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson.Serialization.Conventions;
using NetScheduler.Clients.Abstractions;
using NetScheduler.Clients.Constants;
using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Entities;
using NetScheduler.Models.Cache;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Extensions;
using NetScheduler.Services.Schedules.Abstractions;
using NetScheduler.Services.Schedules.Exceptions;
using NetScheduler.Services.Schedules.Extensions;
using NetScheduler.Services.Tasks.Abstractions;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Xml.Schema;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ITaskService _taskService;
    private readonly IFeatureClient _featureClient;

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        ITaskService taskService,
        IFeatureClient featureClient,
        IDistributedCache distributedCache,
        ILogger<ScheduleService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(scheduleRepository, nameof(scheduleRepository));
        ArgumentNullException.ThrowIfNull(taskService, nameof(taskService));
        ArgumentNullException.ThrowIfNull(featureClient, nameof(featureClient));
        ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));

        _scheduleRepository = scheduleRepository;
        _distributedCache = distributedCache;
        _featureClient = featureClient;
        _taskService = taskService;
        _logger = logger;
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
        CancellationToken token = default)
    {
        _logger.LogInformation(
           "{@Method}: Fetching schedule list",
           Caller.GetName());

        var entities = await _scheduleRepository.GetAll(
            token);

        var schedules = entities.Select(
            x => x.ToDomain());

        _logger.LogInformation(
           "{@Method}: {@Count}: Schedules fetched",
           Caller.GetName(),
           schedules?.Count());

        return schedules;
    }

    public async Task<ScheduleModel> UpdateSchedule(
        ScheduleModel scheduleModel,
        CancellationToken token = default)
    {
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

        var entity = await _scheduleRepository.Get(
            scheduleModel.ScheduleId,
            token);

        if (entity == null)
        {
            _logger.LogError(
                "{@Method}: {@ScheduleId}: {@ScheduleName}: Schedule not found",
                Caller.GetName(),
                scheduleModel.ScheduleId,
                scheduleModel.ScheduleName);

            throw new ScheduleNotFoundException(
                $"No schedule with the ID '{scheduleModel.ScheduleId}' exists");
        }

        // TODO: Move to extension
        entity.ScheduleName = scheduleModel.ScheduleName;
        entity.IsActive = scheduleModel.IsActive;

        entity.Cron = scheduleModel.Cron;
        entity.IncludeSeconds = scheduleModel.IncludeSeconds;
        entity.Links = scheduleModel.Links;

        // Clear timestamps to be recalculated
        entity.NextRuntime = default;
        entity.Queue = Enumerable.Empty<int>();

        entity.LastRuntime = scheduleModel.LastRuntime;
        entity.ModifiedDate = DateTime.Now;

        var updatedSchedule = await _scheduleRepository.Replace(
            entity,
            token);

        _logger.LogInformation(
            "{@Method}: {@Schedule}: Updated schedule",
            Caller.GetName(),
            entity.Serialize());

        return updatedSchedule.ToDomain();
    }

    public async Task DeleteSchedule(string scheduleId, CancellationToken token)
    {
        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: Delete schedule",
           Caller.GetName(),
           scheduleId);

        var entity = await _scheduleRepository.Get(
            scheduleId,
            token);

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
            token);

        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: {@ScheduleName}: {@IsDeleted}: Delete results",
           Caller.GetName(),
           scheduleId,
           entity.ScheduleName,
           result > 0);
    }

    public async Task RunSchedule(string scheduleId, CancellationToken token)
    {
        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: Run schedule",
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
           "{@Method}: {@ScheduleId}: {@Links}: Linked tasks to invoke",
           Caller.GetName(),
           scheduleId,
           schedule.Links?.Count() ?? 0);

        if (schedule.Links != null && schedule.Links.Any())
        {
            await _distributedCache.RemoveAsync(
                CacheKey.ScheduleList,
                token);

            var executionTasks = schedule
                .Links
                .Select(async taskId =>
                {
                    return await _taskService.ExecuteTask(
                        taskId,
                        scheduleId,
                        token);
                });

            var results = await Task.WhenAll(
                executionTasks);
        }
        else
        {
            _logger.LogInformation(
               "{@Method}: {@ScheduleId}: No linked tasks to invoke",
               Caller.GetName(),
               scheduleId);
        }
    }

    public async Task<IEnumerable<TaskExecutionResult>> Poll(CancellationToken token)
    {
        _logger.LogInformation(
           "{@Method}: Polling schedule states",
           Caller.GetName());

        var isEnabled = await _featureClient.EvaluateFeature(
            Feature.NetScheduler);

        if (!isEnabled)
        {
            _logger.LogInformation(
               "{@Method}: Scheduler is disabled",
               Caller.GetName());

            return Enumerable.Empty<TaskExecutionResult>();
        }

        // Get all currently active schedules
        var activeSchedules = await GetActiveSchedulesAsync(
            token);

        var executionQueue = new List<ScheduleModel>();

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

                _logger.LogInformation(
                    "{@Method}: {@ScheduleId}: {@ScheduleName}: {@IsTriggered}: {@TimeRemaining}: Schedule trigger state",
                    Caller.GetName(),
                    schedule.ScheduleId,
                    schedule.ScheduleName,
                    isTriggered,
                    schedule.GetTimeRemaining());

                // If the schedule is triggered add to the
                // execution queue
                if (isTriggered)
                {
                    executionQueue.Add(updatedSchedule);
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
               executionQueue?.Select(x => x.ScheduleId),
               executionQueue?.Select(x => x.ScheduleName));

            return await ProcessExecutionQueue(
                executionQueue,
                token);
        }

        return Enumerable.Empty<TaskExecutionResult>();
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

            schedule.UpdateLastRuntime();

            var updatedSchedule = UpdateScheduleRuntimes(
                schedule);

            await _scheduleRepository.Replace(
                updatedSchedule.ToEntity(),
                token);

            return (true, updatedSchedule);
        }

        return (false, schedule);
    }

    private ScheduleModel UpdateScheduleRuntimes(
        ScheduleModel schedule)
    {
        var expression = schedule.GetCronExpression();

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            "Mountain Standard Time");

        var queue = expression.GetOccurrences(
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7),
            timeZone);

        schedule.Queue = queue.Take(5)
            .Select(x => (int)new DateTimeOffset(x)
            .ToUnixTimeSeconds());

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: {@RuntimeQueue}: Schedule runtimes",
            Caller.GetName(),
            schedule.ScheduleId,
            schedule.ScheduleName,
            schedule.Queue);

        schedule.NextRuntime = (int)new DateTimeOffset(queue
            .FirstOrDefault())
            .ToUnixTimeSeconds();

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

    private async Task<IEnumerable<TaskExecutionResult>> ProcessExecutionQueue(
        IEnumerable<ScheduleModel> executionQueue,
        CancellationToken token)
    {
        _logger.LogInformation(
            "{@Method}: {@QueueLength}: Processing task execution queue",
            Caller.GetName(),
            executionQueue.Count());

        var schedules = executionQueue
            .Where(x => x.Links.Any());

        var scheduleTasks = schedules.SelectMany(
            x => x.Links,
            (schedule, taskId) => new
            {
                ScheduleId = schedule.ScheduleId,
                TaskId = taskId
            });

        _logger.LogInformation(
            "{@Method}: {@ScheduleTasks}: Schedule tasks to execute",
            Caller.GetName(),
            scheduleTasks);

        var tasks = scheduleTasks.Select(async sched =>
        {
            return await _taskService.ExecuteTask(
                sched.TaskId,
                sched.ScheduleId,
                token);
        });


        return await Task.WhenAll(tasks);
    }

    private async Task<IEnumerable<ScheduleModel>> GetActiveSchedulesAsync(CancellationToken token)
    {
        var schedules = await _scheduleRepository.GetAll(
            token);

        var activeSchedules = schedules
            .Where(x => x.IsActive ?? true)
            .ToList();

        return activeSchedules.Select(x => x.ToDomain());
    }

    private async Task InitializeScheduleAsync(ScheduleModel schedule, CancellationToken token)
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
