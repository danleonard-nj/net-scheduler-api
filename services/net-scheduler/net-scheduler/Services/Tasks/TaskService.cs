namespace NetScheduler.Services.Tasks;

using Microsoft.Identity.Web;
using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Models.Events;
using NetScheduler.Models.Http;
using NetScheduler.Models.Identity;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.Extensions;
using NetScheduler.Services.Identity.Exceptions;
using NetScheduler.Services.Schedules.Extensions;
using NetScheduler.Services.Tasks.Abstractions;
using NetScheduler.Services.Tasks.Exceptions;
using System.Text.Json;
using System.Threading.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _scheduleTaskRepository;
    private readonly IEventService _eventService;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository scheduleTaskRepository,
        ITokenAcquisition tokenAcquisition,
        IEventService eventService,
        ILogger<TaskService> logger)
    {
        ArgumentNullException.ThrowIfNull(scheduleTaskRepository, nameof(scheduleTaskRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventService, nameof(eventService));
        ArgumentNullException.ThrowIfNull(tokenAcquisition, nameof(tokenAcquisition));

        _scheduleTaskRepository = scheduleTaskRepository;
        _tokenAcquisition = tokenAcquisition;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task<ScheduleTaskModel> GetTask(
        string taskId,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        var schedule = await _scheduleTaskRepository.Get(
            taskId,
            token);

        return schedule?.ToDomain();
    }

    public async Task<ScheduleTaskModel> CreateTask(
        CreateTaskModel createTaskModel,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(createTaskModel, nameof(createTaskModel));

        _logger.LogInformation(
            "{@Method}: {@CreateTaskModel}: Creating task",
            Caller.GetName(),
            createTaskModel);

        var scheduleTaskModel = createTaskModel.ToDomain();

        var scheduleTask = scheduleTaskModel
            .ToScheduleTask();

        await _scheduleTaskRepository.Insert(
            scheduleTask,
            token);

        _logger.LogInformation(
            "{@Method}: {@ScheduleTask}: Created task",
            Caller.GetName(),
            createTaskModel);

        return scheduleTaskModel;
    }

    public async Task<IEnumerable<ScheduleTaskModel>> GetTasks(
        CancellationToken token = default)
    {
        var schedules = await _scheduleTaskRepository.GetAll(token);
        var models = schedules.Select(x => x.ToDomain());

        return models;
    }

    public async Task<ScheduleTaskModel> UpsertTask(
        ScheduleTaskModel scheduleTaskModel,
        CancellationToken token = default)
    {
        _logger.LogInformation(
            "{@Method}: {@ScheduleTaskModel}: Upserting task",
            Caller.GetName(),
            scheduleTaskModel);

        if (!string.IsNullOrEmpty(scheduleTaskModel.TaskId))
        {
            var existingSchedule = await _scheduleTaskRepository.Get(
                scheduleTaskModel.TaskId,
                token);

            if (existingSchedule != null)
            {
                _logger.LogInformation(
                   "{@Method}: {@TaskId}: Task exists, updating task",
                   Caller.GetName(),
                   scheduleTaskModel.TaskId);

                var task = scheduleTaskModel.ToScheduleTask();

                var updatedSchedule = await _scheduleTaskRepository.Replace(
                    task,
                    token);

                return updatedSchedule.ToDomain();
            }
        }

        var createTask = scheduleTaskModel.ToScheduleTask(
            Guid.NewGuid().ToString());

        _logger.LogInformation(
            "{@Method}: {@ScheduleTaskModel}: Creating task",
            Caller.GetName(),
            createTask);

        var createdTask = await _scheduleTaskRepository.Insert(
            createTask,
            token);

        return createdTask.ToDomain();
    }

    public async Task DeleteTask(
        string taskId,
        CancellationToken token)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        _logger.LogInformation(
            "{@Method}: {@TaskId}",
            Caller.GetName(),
            taskId);

        var exists = await _scheduleTaskRepository.Get(
            taskId,
            token);

        if (exists == null)
        {
            throw new TaskNotFoundException($"No task with the ID '{taskId}' exists");
        }

        var count = await _scheduleTaskRepository.Delete(
            taskId,
            token);

        _logger.LogInformation(
            "{@Method}: {@TaskId}: Deleted record count: {@RecordCount}",
            Caller.GetName(),
            taskId,
            count);
    }

    public async Task<IEnumerable<ScheduleTaskModel>> ExecuteTasksAsync(
        IEnumerable<string> taskIds,
        string scheduleId,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(scheduleId))
        {
            throw new ArgumentNullException(nameof(scheduleId));
        }

        if (!taskIds.Any())
        {
            _logger.LogInformation(
                "{@Method}: {@ScheduleId}: No associated tasks to execute",
                Caller.GetName(),
                scheduleId);

            return Enumerable.Empty<ScheduleTaskModel>();
        }

        var entities = await _scheduleTaskRepository.GetTasksAsync(
            taskIds,
            token);

        var scheduleTasks = entities
            .Select(x => x.ToDomain());

        var tasksByClient = scheduleTasks
            .GroupBy(x => x.IdentityClientId)
            .ToDictionary(k => k.Key, v => v.ToList());

        var dispatchTasks = new List<Task>();

        foreach (var (clientId, tasks) in tasksByClient)
        {
            var eventMessages = tasks.Select(
                x => CreateEventMessage(x));

            var dispatch = _eventService.DispatchEventsAsync(
                eventMessages,
                clientId,
                token);

            dispatchTasks.Add(dispatch);
        };

        await Task.WhenAll(dispatchTasks);

        return scheduleTasks;
    }

    public async Task<TokenModel> GetTokenAsync(string appId)
    {
        var token = await _tokenAcquisition.GetAccessTokenForAppAsync(appId);

        return new TokenModel(token);
    }

    private ApiEvent CreateEventMessage(ScheduleTaskModel task)
    {
        if (string.IsNullOrWhiteSpace(task.IdentityClientId))
        {
            _logger.LogError(
                "{@Method}: {@TaskId}: {@TaskName}: No identity client configured",
                Caller.GetName(),
                task.TaskId,
                task.TaskName);

            throw new InvalidIdentityClientException(
                $"No idenity client is configured for task '{task.TaskName}'");
        }

        if (string.IsNullOrWhiteSpace(task.Endpoint))
        {
            _logger.LogError(
                "{@Method}: {@TaskId}: {@TaskName}: Invalid request configuration",
                Caller.GetName(),
                task.TaskId,
                task.TaskName);

            throw new InvalidTaskException("Task endpoint is not defined");
        }

        if (string.IsNullOrWhiteSpace(task.Method))
        {
            _logger.LogError(
                "{@Method}: {@TaskId}: {@TaskName}: Invalid request configuration",
                Caller.GetName(),
                task.TaskId,
                task.TaskName);

            throw new InvalidTaskException("Task request method is not defined");
        }

        var taskEventMessage = task.ToApiEvent();

        _logger.LogInformation(
           "{@Method}: {@TaskId}: {@TaskName}: {@ApiEvent}: Event to dispatch",
           Caller.GetName(),
           task.TaskId,
           task.TaskName,
           taskEventMessage);

        return taskEventMessage;
    }
}