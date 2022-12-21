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
             var exists = (await _scheduleTaskRepository
                .Query(x => x.TaskId == scheduleTaskModel.TaskId, token))
                .Any();

            if (exists)
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

        var exists = await _scheduleTaskRepository.Query(
            x => x.TaskId == taskId,
            token);

        if (exists == null || !exists.Any())
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

    private async Task<TaskExecutionResult> InvokeTaskExecution(
        string taskId,
        string scheduleId,
        CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        _logger.LogInformation(
            "{@TaskId}: Execute task",
            taskId);

        var task = await GetTask(taskId, token);

        if (task == null)
        {
            _logger.LogError(
                "{@TaskId}: Task does not exist",
                taskId);

            throw new TaskException("Task not found");
        }

        _logger.LogInformation(
            "{@TaskId}: {@Task}: Task data",
            taskId,
            JsonSerializer.Serialize(task));

        if (task.IdentityClientId == null)
        {
            _logger.LogError(
                "{@TaskId}: Task identity client is not configured",
                taskId);

            throw new InvalidIdentityClientException(
                $"Task '{task.TaskName}' has no valid client configuration");
        }

        _logger.LogInformation(
            "{@TaskId}: {@TaskName}: {@IdentityClient}: Fetching access token",
            taskId,
            task.TaskName,
            task.IdentityClientId);

        var authToken = await _tokenAcquisition.GetAccessTokenForAppAsync(
            task.IdentityClientId);

        _logger.LogInformation(
           "{@TaskId}: {@TaskName}: {@Token}: Task access token fetched successfully",
           taskId,
           task.TaskName,
           authToken);

        var authHeaders = new AuthorizationHeaders(
            authToken);

        _logger.LogInformation(
            "{@TaskId}: {@AuthHeaders}: Task auth headers",
            taskId,
            JsonSerializer.Serialize(authHeaders));

        if (task.Endpoint == null || task.Method == null)
        {
            _logger.LogError(
               "{@TaskId}: {@TaskName}: Invalid request configuration",
               taskId,
               task.TaskName);

            throw new TaskException($"Task '{task.TaskName}' is not a valid request");
        }

        var eventMessage = new ApiEvent
        {
            Endpoint = task.Endpoint,
            Json = task.Payload,
            Method = task.Method,
            Headers = authHeaders,
            EventKey = $"{scheduleId}-{taskId}",
            TaskId = taskId,
            ScheduleId = scheduleId
        };

        _logger.LogInformation(
            "{@TaskId}: {@TaskName}: {@EventMessage}: Created task event message",
            taskId,
            task.TaskName,
            JsonSerializer.Serialize(eventMessage));

        await _eventService.Send(eventMessage
            .ToServiceBusMessage());

        _logger.LogInformation(
            "{@TaskId}: {@TaskName}: Event message sent",
            taskId,
            task.TaskName);

        return new TaskExecutionResult(
            taskId);
    }

    public async Task<TaskExecutionResult> ExecuteTask(
        string taskId,
        string scheduleId,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new ArgumentNullException(nameof(taskId));
        }

        try
        {
            return await InvokeTaskExecution(
                taskId,
                scheduleId,
                token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{@Method}: {@TaskId}: {@Message}: Failed to execute task",
                Caller.GetName(),
                taskId,
                ex.Message);
        }

        return new TaskExecutionResult(taskId)
        {
            ExecutionResult = null
        };
    }

    public async Task<TokenModel> GetTokenAsync(string appId)
    {
        var token = await _tokenAcquisition.GetAccessTokenForAppAsync(appId);

        return new TokenModel(token);
    }
}