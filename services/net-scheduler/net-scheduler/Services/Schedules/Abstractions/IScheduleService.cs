namespace NetScheduler.Services.Schedules.Abstractions;
using NetScheduler.Models.Schedules;
using NetScheduler.Models.Tasks;

public interface IScheduleService
{
    Task<ScheduleModel> CreateSchedule(CreateScheduleModel createScheduleModel, CancellationToken token);

    Task DeleteSchedule(string scheduleId, CancellationToken token);

    Task<ScheduleModel> GetSchedule(string scheduleId, CancellationToken token);

    Task<IEnumerable<ScheduleModel>> GetSchedules(CancellationToken token);

    Task<IEnumerable<TaskExecutionResult>> Poll(CancellationToken token);

    Task<ScheduleModel> UpdateSchedule(ScheduleModel scheduleModel, CancellationToken token);

    Task RunSchedule(string scheduleId, CancellationToken token);
}
