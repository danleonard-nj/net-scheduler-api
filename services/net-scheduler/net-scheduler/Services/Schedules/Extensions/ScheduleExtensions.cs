namespace NetScheduler.Services.Schedules.Extensions;
using Cronos;
using NetScheduler.Data.Entities;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Schedules.Helpers;
using System.Text.Json;

public static class ScheduleExtensions
{
    public static ScheduleModel ToDomain(this ScheduleItem schedule)
    {
        return new ScheduleModel
        {
            ScheduleId = schedule.ScheduleId,
            ScheduleName = schedule.ScheduleName,
            Cron = schedule.Cron,
            IncludeSeconds = schedule.IncludeSeconds,
            LastRuntime = schedule.LastRuntime,
            NextRuntime = schedule.NextRuntime,
            Links = schedule.Links,
            Queue = schedule.Queue,
            ModifiedDate = schedule.ModifiedDate,
            ScheduleType = schedule.ScheduleType,
            CreatedDate = schedule.CreatedDate,
            IsActive = schedule.IsActive
        };
    }

    public static TaskModel ToDomain(this TaskItem scheduleAction)
    {
        return new TaskModel
        {
            Endpoint = scheduleAction.Endpoint,
            IdentityClientId = scheduleAction.IdentityClientId,
            Method = scheduleAction.Method,
            Payload = scheduleAction.Payload != null ? JsonSerializer.Deserialize<object>(scheduleAction.Payload!) : null,
            TaskId = scheduleAction.TaskId,
            TaskName = scheduleAction.TaskName
        };
    }

    public static TaskModel ToDomain(this CreateTaskModel createTaskModel)
    {
        return new TaskModel
        {
            Endpoint = createTaskModel.Endpoint,
            IdentityClientId = createTaskModel.IdentityClientId,
            Method = createTaskModel.Method,
            Payload = createTaskModel.Payload,
            TaskId = Guid.NewGuid().ToString(),
            TaskName = createTaskModel.TaskName
        };
    }

    public static ScheduleItem ToEntity(this ScheduleModel scheduleModel)
    {
        ArgumentNullException.ThrowIfNull(scheduleModel, nameof(scheduleModel));

        return new ScheduleItem
        {
            ScheduleName = scheduleModel.ScheduleName,
            ScheduleId = scheduleModel.ScheduleId,
            IncludeSeconds = scheduleModel.IncludeSeconds,
            Cron = scheduleModel.Cron,
            LastRuntime = scheduleModel.LastRuntime,
            NextRuntime = scheduleModel.NextRuntime,
            Links = scheduleModel.Links,
            Queue = scheduleModel.Queue,
            ModifiedDate = scheduleModel.ModifiedDate,
            CreatedDate = scheduleModel.CreatedDate,
            ScheduleType = scheduleModel.ScheduleType,
            IsActive = scheduleModel.IsActive
        };
    }

    public static ScheduleModel UpdateScheduleDetails(this ScheduleModel source, ScheduleModel updated)
    {
        source.ScheduleName = updated.ScheduleName;
        source.IsActive = updated.IsActive;

        source.Cron = updated.Cron;
        source.IncludeSeconds = updated.IncludeSeconds;
        source.Links = updated.Links;

        // Clear timestamps to be recalculated
        source.NextRuntime = default;
        source.Queue = Enumerable.Empty<int>();

        source.LastRuntime = updated.LastRuntime;
        source.ModifiedDate = DateTime.Now;

        return source;
    }

    public static TriggeredScheduleModel ToTriggeredScheduleModel(
        this ScheduleModel schedule,
        bool isManual = false)
    {
        return new TriggeredScheduleModel
        {
            Schedule = schedule,
            IsManual = isManual
        };
    }

    public static TaskItem ToScheduleTask(
        this TaskModel scheduleActionModel,
        string? taskId = null)
    {
        ArgumentNullException.ThrowIfNull(scheduleActionModel, nameof(scheduleActionModel));

        return new TaskItem
        {
            Endpoint = scheduleActionModel.Endpoint,
            IdentityClientId = scheduleActionModel.IdentityClientId,
            Method = scheduleActionModel.Method,
            Payload = scheduleActionModel.Payload != null 
                ? JsonSerializer.Serialize(scheduleActionModel.Payload) 
                : null,
            TaskId = taskId ?? scheduleActionModel.TaskId,
            TaskName = scheduleActionModel.TaskName
        };
    }

    public static CronExpression GetCronExpression(this ScheduleModel schedule)
    {
        if (!CronExpressionParser.TryParse(
            schedule.Cron,
            schedule.IncludeSeconds,
            out var expression))
        {
            throw new ArgumentException($"Invalid CRON expression: {schedule.Cron}", nameof(schedule.Cron));
        }

        return expression;
    }

    public static bool GetScheduleInvocationState(this ScheduleModel schedule)
    {
        return schedule.NextRuntime != default && DateTimeOffset.UtcNow
            .ToUnixTimeSeconds() >= schedule.NextRuntime;
    }

    public static void UpdateLastRuntime(this ScheduleModel schedule)
    {
        schedule.LastRuntime = (int)DateTimeOffset
            .UtcNow
            .ToUnixTimeSeconds();
    }

    public static TimeSpan GetTimeRemaining(this ScheduleModel schedule)
    {
        return DateTimeOffset.FromUnixTimeSeconds(
            schedule.NextRuntime) - DateTimeOffset.UtcNow;
    }
}