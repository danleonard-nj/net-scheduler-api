namespace NetScheduler.Services.Schedules.Extensions;
using Cronos;
using NetScheduler.Data.Models;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Extensions;
using System.Text.Json;

public static class ScheduleExtensions
{
    public static ScheduleModel ToDomain(this Schedule schedule)
    {
        return new ScheduleModel
        {
            ScheduleId = schedule.ScheduleId,
            ScheduleName = schedule.ScheduleName,
            Cron = schedule.Cron,
            IncludeSeconds = schedule.IncludeSeconds,
            LastRuntime = schedule.LastRuntime?.ToLocalDateTime(),
            NextRuntime = schedule.NextRuntime?.ToLocalDateTime(),
            Links = schedule.Links,
            Queue = schedule.Queue.Select(x => x.ToLocalDateTime()),
            UpdatedDateTime = schedule.UpdateDateTime,
            IsActive = schedule.IsActive
        };
    }

    public static ScheduleTaskModel ToDomain(this ScheduleTask scheduleAction)
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

    public static Schedule ToSchedule(this ScheduleModel scheduleModel)
    {
        ArgumentNullException.ThrowIfNull(scheduleModel, nameof(scheduleModel));

        return new Schedule
        {
            ScheduleName = scheduleModel?.ScheduleName,
            ScheduleId = scheduleModel?.ScheduleId,
            IncludeSeconds = scheduleModel.IncludeSeconds,
            Cron = scheduleModel.Cron,
            LastRuntime = (int?)scheduleModel.LastRuntime?.ToUnixTimeSeconds(),
            NextRuntime = (int?)scheduleModel.NextRuntime?.ToUnixTimeSeconds(),
            Links = scheduleModel.Links,
            Queue = scheduleModel.Queue.Select(x => (int)x.ToUnixTimeSeconds()),
            UpdateDateTime = scheduleModel.UpdatedDateTime,
            IsActive = scheduleModel.IsActive
        };
    }

    public static ScheduleTask ToScheduleTask(
        this ScheduleTaskModel scheduleActionModel,
        string? taskId = null)
    {
        ArgumentNullException.ThrowIfNull(scheduleActionModel, nameof(scheduleActionModel));

        return new ScheduleTask
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

    public static CronExpression GetCronExpression(this Schedule schedule)
    {
        var cronFormat = schedule.IncludeSeconds
            ? CronFormat.IncludeSeconds
            : CronFormat.Standard;

        return CronExpression.Parse(schedule.Cron, cronFormat);
    }

    public static bool GetScheduleInvocationState(this Schedule schedule)
    {
        return schedule.NextRuntime != null && DateTimeOffset.UtcNow
            .ToUnixTimeSeconds() >= schedule.NextRuntime;
    }

    public static void UpdateLastRuntime(this Schedule schedule)
    {
        schedule.LastRuntime = (int?)DateTimeOffset
            .UtcNow
            .ToUnixTimeSeconds();
    }
}
