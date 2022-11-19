namespace NetScheduler.Services.Schedules;

using Microsoft.Extensions.Caching.Distributed;
using NetScheduler.Clients.Abstractions;
using NetScheduler.Clients.Constants;
using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Models;
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

        return schedule.ToDomain();
    }

    public async Task<ScheduleModel> CreateSchedule(
        CreateScheduleModel createScheduleModel,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(createScheduleModel, nameof(createScheduleModel));

        _logger.LogInformation(
            "{@Method}: Create schedule",
            Caller.GetName());

        await _distributedCache.RemoveAsync(
            CacheKey.ScheduleList,
            token);

        var scheduleModel = createScheduleModel
            .ToDomain();

        var schedule = scheduleModel
            .ToSchedule();

        _logger.LogInformation(
            "{@Method}: {@Schedule}",
            Caller.GetName(),
            schedule.Serialize());

        await _scheduleRepository.Insert(
            schedule,
            token);

        return scheduleModel;
    }

    public async Task<IEnumerable<ScheduleModel>> GetSchedules(
        CancellationToken token = default)
    {
        var cacheValue = await _distributedCache.GetAsync<IEnumerable<ScheduleModel>>(
            CacheKey.ScheduleList,
            token);

        if (cacheValue != null)
        {
            _logger.LogInformation(
                "{@Method}: Returning schedule list from cache",
                Caller.GetName());

            return cacheValue;
        }

        var schedules = await _scheduleRepository.GetAll(
            token);

        var models = schedules.Select(
            x => x.ToDomain());

        await _distributedCache.SetAsync(
            CacheKey.ScheduleList,
            models,
            CacheOptions.ExpirationHours(24),
            token);

        return models;
    }

    public async Task<ScheduleModel> UpsertSchedule(
        ScheduleModel scheduleModel,
        CancellationToken token = default)
    {
        _logger.LogInformation(
            "{@Method}: {@Schedule}: Upsert schedule",
            Caller.GetName(),
            scheduleModel.Serialize());

        _logger.LogInformation(
            "{@Method}: {@Schedule}: Removing schedule from cache",
            Caller.GetName(),
            scheduleModel.Serialize());

        await _distributedCache.RemoveAsync(
            CacheKey.ScheduleList,
            token);

        if (string.IsNullOrEmpty(scheduleModel.ScheduleId))
        {
            _logger.LogError(
                "{@Method}: {@Schedule}: Invalid schedule ID",
                Caller.GetName(),
                scheduleModel.Serialize());

            throw new ScheduleNotFoundException(
                $"Schedule ID '{scheduleModel.ScheduleId}' is not valid");
        }

        var exists = await _scheduleRepository.Get(
            scheduleModel.ScheduleId,
            token) != null;

        if (exists)
        {
            _logger.LogInformation(
                "{@Method}: Schedule exists, performing update",
                Caller.GetName());

            var schedule = scheduleModel.ToSchedule();

            var updatedSchedule = await _scheduleRepository.Update(
                schedule,
                token);

            _logger.LogInformation(
                "{@Method}: {@Schedule}: Updated schedule",
                Caller.GetName(),
                schedule.Serialize());

            return updatedSchedule.ToDomain();
        }

        _logger.LogInformation(
            "{@Method}: Schedule does not exist, performing insert",
            Caller.GetName());

        var createSchedule = scheduleModel
            .ToSchedule();

        createSchedule.ScheduleId = Guid
            .NewGuid()
            .ToString();

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: Created schedule ID",
            Caller.GetName(),
            createSchedule.ScheduleId);

        var createdSchedule = await _scheduleRepository.Insert(
            createSchedule,
            token);

        _logger.LogInformation(
           "{@Method}: {@Schedule}: Created schedule",
           Caller.GetName(),
           createdSchedule.Serialize());

        return createdSchedule.ToDomain();
    }

    public async Task DeleteSchedule(string scheduleId, CancellationToken token)
    {
        _logger.LogInformation(
           "{@Method}: {@ScheduleId}: Delete schedule",
           Caller.GetName(),
           scheduleId);

        await _distributedCache.RemoveAsync(
            CacheKey.ScheduleList,
            token);

        var exists = await _scheduleRepository.Get(
            scheduleId,
            token);

        if (exists == null)
        {
            throw new ScheduleNotFoundException(
                $"No schedule with the ID '{scheduleId}' exists");
        }

        var result = await _scheduleRepository.Delete(
            scheduleId,
            token);

        _logger.LogInformation(
           "{@Method}: {@DeleteCount}",
           Caller.GetName(),
           result);
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

        var executionQueue = new List<Schedule>();

        _logger.LogInformation(
           "{@Method}: {@ScheduleCount}: Schedules to process",
           Caller.GetName(),
           activeSchedules.Count());

        foreach (var schedule in activeSchedules)
        {
            _logger.LogInformation(
                "{@Method}: {@ScheduleId}: {@ScheduleName}: Evaluating schedule trigger",
                Caller.GetName(),
                schedule.ScheduleId,
                schedule.ScheduleName);

            try
            {
                var result = await EvaluateScheduleTriggerAsync(
                    schedule,
                    token);

                // If the schedule is triggered add to the
                // execution queue
                if (result != null)
                {
                    executionQueue.Add(result);
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
               "{@Method}: {@ScheduleCount}: Schedules queued for execution",
               Caller.GetName(),
               executionQueue.Count());

            await _distributedCache.RemoveAsync(
                CacheKey.ScheduleList,
                token);

            _logger.LogInformation(
               "{@Method}: Schedule cache cleared",
               Caller.GetName());

            return await ProcessExecutionQueue(
                executionQueue,
                token);
        }

        return Enumerable.Empty<TaskExecutionResult>();
    }

    private async Task<Schedule?> EvaluateScheduleTriggerAsync(Schedule schedule, CancellationToken token)
    {
        // Set initial values for new schedules
        if (schedule.NextRuntime == null)
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

            // Remove when all active flags are set
            if (schedule.IsActive == null)
            {
                schedule.IsActive = true;
            }

            var updatedSchedule = UpdateScheduleRuntimes(
                schedule);

            await _scheduleRepository.Update(
                updatedSchedule,
                token);

            return updatedSchedule;
        }

        return null;
    }

    private Schedule UpdateScheduleRuntimes(
        Schedule schedule)
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

        schedule.NextRuntime = (int)new DateTimeOffset(queue
            .FirstOrDefault())
            .ToUnixTimeSeconds();

        _logger.LogInformation(
            "{@Method}: {@Schedule}: Updated invoked schedule",
            Caller.GetName(),
            schedule.Serialize());

        schedule.UpdateDateTime = DateTime.UtcNow;
        return schedule;
    }

    private async Task<IEnumerable<TaskExecutionResult>> ProcessExecutionQueue(
        IEnumerable<Schedule> executionQueue,
        CancellationToken token)
    {
        _logger.LogInformation(
            "{@Method}: {@QueueLength}: Task execution queue",
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

        var tasks = scheduleTasks.Select(async sched =>
        {
            return await _taskService.ExecuteTask(
                sched.TaskId,
                sched.ScheduleId,
                token);
        });


        return await Task.WhenAll(tasks);
    }

    private async Task<IEnumerable<Schedule>> GetActiveSchedulesAsync(CancellationToken token)
    {
        var schedules = await _scheduleRepository.GetAll(
            token);

        var activeSchedules = schedules
            .Where(x => x.IsActive ?? true)
            .ToList();

        return activeSchedules;
    }

    private async Task InitializeScheduleAsync(Schedule schedule, CancellationToken token)
    {
        _logger.LogInformation(
            "{@Method}: {@ScheduleName}: Initial update for schedule trigger values",
            Caller.GetName(),
            schedule.ScheduleName);

        UpdateScheduleRuntimes(schedule);

        await _scheduleRepository.Update(
            schedule,
            token);
    }
}
