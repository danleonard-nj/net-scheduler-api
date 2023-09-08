namespace NetScheduler.Services.Tasks;

using Microsoft.Identity.Web;
using NetScheduler.Configuration;
using NetScheduler.Data.Abstractions;
using NetScheduler.Models.Events;
using NetScheduler.Models.Identity;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.Identity.Abstractions;
using NetScheduler.Services.Identity.Exceptions;
using NetScheduler.Services.Schedules.Extensions;
using NetScheduler.Services.Tasks.Abstractions;
using NetScheduler.Services.Tasks.Exceptions;
using NetScheduler.Services.Tasks.Extensions;
using System.Threading.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IEventService _eventService;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IIdentityService _identityService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository scheduleTaskRepository,
        ITokenAcquisition tokenAcquisition,
        IEventService eventService,
        IIdentityService identityService,
        ILogger<TaskService> logger)
    {
        ArgumentNullException.ThrowIfNull(scheduleTaskRepository, nameof(scheduleTaskRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventService, nameof(eventService));
        ArgumentNullException.ThrowIfNull(tokenAcquisition, nameof(tokenAcquisition));
        ArgumentNullException.ThrowIfNull(identityService, nameof(identityService));

        _identityService = identityService;
        _taskRepository = scheduleTaskRepository;
        _tokenAcquisition = tokenAcquisition;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task<TaskModel> GetTask(
        string taskId,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        var schedule = await _taskRepository.Get(
            taskId,
            token);

        return schedule?.ToDomain();
    }

    public async Task<TaskModel> CreateTask(
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

        await _taskRepository.Insert(
            scheduleTask,
            token);

        _logger.LogInformation(
            "{@Method}: {@ScheduleTask}: Created task",
            Caller.GetName(),
            createTaskModel);

        return scheduleTaskModel;
    }

    public async Task<IEnumerable<TaskModel>> GetTasks(
        CancellationToken cancellationToken = default)
    {
        var schedules = await _taskRepository.GetAll(
            cancellationToken);

        var models = schedules.Select(x => x.ToDomain());

        return models;
    }

    public async Task<TaskModel> UpsertTask(
        TaskModel scheduleTaskModel,
        CancellationToken token = default)
    {
        // TODO: Switch to explicit updates and inserts
        _logger.LogInformation(
            "{@Method}: {@ScheduleTaskModel}: Upserting task",
            Caller.GetName(),
            scheduleTaskModel);

        if (!string.IsNullOrEmpty(scheduleTaskModel.TaskId))
        {
            var existingSchedule = await _taskRepository.Get(
                scheduleTaskModel.TaskId,
                token);

            if (existingSchedule != null)
            {
                _logger.LogInformation(
                   "{@Method}: {@TaskId}: Task exists, updating task",
                   Caller.GetName(),
                   scheduleTaskModel.TaskId);

                var task = scheduleTaskModel.ToScheduleTask();

                var updatedSchedule = await _taskRepository.Replace(
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

        var createdTask = await _taskRepository.Insert(
            createTask,
            token);

        return createdTask.ToDomain();
    }

    public async Task DeleteTask(
        string taskId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        _logger.LogInformation(
            "{@Method}: {@TaskId}",
            Caller.GetName(),
            taskId);

        var exists = await _taskRepository.Get(
            taskId,
            cancellationToken);

        if (exists == null)
        {
            throw new TaskNotFoundException($"No task with the ID '{taskId}' exists");
        }

        var count = await _taskRepository.Delete(
            taskId,
            cancellationToken);

        _logger.LogInformation(
            "{@Method}: {@TaskId}: Deleted record count: {@RecordCount}",
            Caller.GetName(),
            taskId,
            count);
    }

    public async Task<IEnumerable<(TaskModel task, string invocationId)>> ExecuteTasksAsync(
        IEnumerable<string> tasks,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(tasks, nameof(tasks));

        _logger.LogInformation(
            "{@Method}: {@TaskIds}: Executing tasks",
            Caller.GetName(),
            tasks);

        if (!tasks.Any())
        {
            _logger.LogInformation(
                "{@Method}: No associated tasks to execute",
                Caller.GetName());

            return Enumerable.Empty<(TaskModel, string)>();
        }

        var results = new List<(TaskModel, string)>();

        // Fetch the tasks from the database
        var scheduleTasks = await GetTasksAsync(tasks, token);

        var clientIds = scheduleTasks
            .Select(x => x.IdentityClientId)
            .Distinct();

        // Fetch the tokens for the identity clients configured
        // to run the invoked tasks
        var clientTokens = await _identityService.GetClientTokensAsync(
            clientIds,
            token);

        var eventMessages = new List<ApiEvent>();

        foreach (var task in scheduleTasks.DistinctBy(x => x.TaskId))
        {
            // Get the token for the identity client configured
            // for the task
            if (!clientTokens.TryGetValue(task.IdentityClientId, out var clientToken))
            {
                // In case we fail to fetch a token for an identity client
                // in the list of schedule tasks
                throw new InvalidIdentityClientTokenException(
                    $"No token fetched for client {task.IdentityClientId}");
            }

            var invocationId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "{@Method}: {@TaskId}: {@TaskName}: {@InvocationId}: Creating service bus event",
                Caller.GetName(),
                task.TaskId,
                task.TaskName,
                invocationId);

            // Create service bus event to run the triggered
            // task
            var eventMessage = CreateEventMessage(
                task,
                clientToken,
                invocationId);

            eventMessages.Add(eventMessage);
            results.Add((task, invocationId));
        }

        _logger.LogInformation(
            "{@Method}: {@TaskIds}: {@EventCount}: Dispatching events",
            Caller.GetName(),
            tasks,
            eventMessages?.Count() ?? 0);

        // Dispatch the events to run the tasks
        await _eventService.DispatchEventsAsync(
            eventMessages,
            token);

        _logger.LogInformation(
            "{@Method}: {@TaskIds}: {@EventCount}: Dispatched events",
            Caller.GetName(),
            tasks,
            eventMessages?.Count() ?? 0);

        return results;
    }

    public async Task<TokenModel> GetTokenAsync(string appId)
    {
        var token = await _tokenAcquisition.GetAccessTokenForAppAsync(appId);

        return new TokenModel(token);
    }

    private async Task<IEnumerable<TaskModel>> GetTasksAsync(
        IEnumerable<string> taskIds,
        CancellationToken cancellationToken = default)
    {
        // Fetch the tasks from the database
        var entities = await _taskRepository.GetTasksAsync(
            taskIds,
            cancellationToken);

        var scheduleTasks = entities
            .Select(x => x.ToDomain());

        return scheduleTasks;
    }

    private ApiEvent CreateEventMessage(
        TaskModel task,
        string token,
        string invocationId)
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

            throw new InvalidTaskException($"Endpoint for task '{task.TaskId}' is not defined");
        }

        if (string.IsNullOrWhiteSpace(task.Method))
        {
            _logger.LogError(
                "{@Method}: {@TaskId}: {@TaskName}: Invalid request configuration",
                Caller.GetName(),
                task.TaskId,
                task.TaskName);

            throw new InvalidTaskException($"Request method for task '{task.TaskId}' is not defined");
        }

        var taskEventMessage = task.ToApiEvent(
            token,
            invocationId);

        _logger.LogInformation(
           "{@Method}: {@TaskId}: {@TaskName}: {@ApiEvent}: Event to dispatch",
           Caller.GetName(),
           task.TaskId,
           task.TaskName,
           taskEventMessage);

        return taskEventMessage;
    }
}