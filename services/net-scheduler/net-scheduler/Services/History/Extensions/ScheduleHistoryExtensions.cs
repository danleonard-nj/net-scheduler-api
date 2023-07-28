namespace NetScheduler.Services.History.Extensions;

using NetScheduler.Data.Entities;
using NetScheduler.Models.History;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;

public static class ScheduleHistoryExtensions
{
    public static ScheduleHistoryModel ToScheduleHistoryModel(
        this ScheduleModel scheduleModel,
        IEnumerable<(TaskModel task, string invocationId)> tasks,
        int scheduleRuntime,
        bool isManualTrigger = false)
    {
        var taskHistoryEntries = tasks.Select(task => new ScheduleTaskHistoryModel
        {
            ScheduleHistoryTaskId = Guid.NewGuid().ToString(),
            TaskId = task.task.TaskId,
            TaskName = task.task.TaskName,
            InvocationId = task.invocationId
        });

        return new ScheduleHistoryModel
        {
            ScheduleHistoryId = Guid.NewGuid().ToString(),
            ScheduleId = scheduleModel.ScheduleId,
            ScheduleName = scheduleModel.ScheduleName,
            Tasks = taskHistoryEntries,
            IsManualTrigger = isManualTrigger,
            TriggerDate = scheduleRuntime,
            CreatedDate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public static ScheduleHistoryItem ToEntity(this ScheduleHistoryModel scheduleHistoryModel)
    {
        return new ScheduleHistoryItem
        {
            ScheduleHistoryId = scheduleHistoryModel.ScheduleHistoryId,
            ScheduleId = scheduleHistoryModel.ScheduleId,
            ScheduleName = scheduleHistoryModel.ScheduleName,
            IsManualTrigger = scheduleHistoryModel.IsManualTrigger,
            Tasks = scheduleHistoryModel.Tasks.Select(scheduleHistoryModelTask => new ScheduleHistoryTaskItem
            {
                ScheduleHistoryTaskId = scheduleHistoryModelTask.ScheduleHistoryTaskId,
                TaskId = scheduleHistoryModelTask.TaskId,
                TaskName = scheduleHistoryModelTask.TaskName
            }),
            TriggerDate = scheduleHistoryModel.TriggerDate,
            CreatedDate = scheduleHistoryModel.CreatedDate
        };
    }

    public static ScheduleHistoryModel ToDomain(this ScheduleHistoryItem scheduleHistoryItem)
    {
        return new ScheduleHistoryModel
        {
            ScheduleHistoryId = scheduleHistoryItem.ScheduleHistoryId,
            ScheduleId = scheduleHistoryItem.ScheduleId,
            ScheduleName = scheduleHistoryItem.ScheduleName,
            IsManualTrigger = scheduleHistoryItem.IsManualTrigger,
            Tasks = scheduleHistoryItem.Tasks.Select(scheduleHistoryItemTask => new ScheduleTaskHistoryModel
            {
                ScheduleHistoryTaskId = scheduleHistoryItemTask.ScheduleHistoryTaskId,
                TaskId = scheduleHistoryItemTask.TaskId,
                TaskName = scheduleHistoryItemTask.TaskName
            }),
            TriggerDate = scheduleHistoryItem.TriggerDate,
            CreatedDate = scheduleHistoryItem.CreatedDate
        };
    }
}
