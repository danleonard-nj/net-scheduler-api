namespace NetScheduler.Services.Schedules.Extensions;
using Cronos;
using NetScheduler.Data.Entities;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Extensions;
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
            IsActive = schedule.IsActive
        };
    }

    public static ScheduleTaskModel ToDomain(this ScheduleTaskItem scheduleAction)
    {
        return new ScheduleTaskModel
        {
            Endpoint = scheduleAction.Endpoint,
            IdentityClientId = scheduleAction.IdentityClientId,
            Method = scheduleAction.Method,
            Payload = scheduleAction.Payload != null ? JsonSerializer.Deserialize<object>(scheduleAction.Payload!) : null,
            TaskId = scheduleAction.TaskId,
            TaskName = scheduleAction.TaskName
        };
    }

    public static ScheduleModel ToDomain(this CreateScheduleModel createScheduleModel)
    {
        return new ScheduleModel
        {
            Cron = createScheduleModel.Cron,
            IncludeSeconds = createScheduleModel.IncludeSeconds,
            ScheduleId = Guid.NewGuid().ToString(),
            ScheduleName = createScheduleModel.ScheduleName,
        };
    }

    public static ScheduleTaskModel ToDomain(this CreateTaskModel createTaskModel)
    {
        return new ScheduleTaskModel
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
            IsActive = scheduleModel.IsActive
        };
    }

    public static ScheduleTaskItem ToScheduleTask(
        this ScheduleTaskModel scheduleActionModel,
        string? taskId = null)
    {
        ArgumentNullException.ThrowIfNull(scheduleActionModel, nameof(scheduleActionModel));

        return new ScheduleTaskItem
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
        var cronFormat = schedule.IncludeSeconds
            ? CronFormat.IncludeSeconds
            : CronFormat.Standard;

        return CronExpression.Parse(schedule.Cron, cronFormat);
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
