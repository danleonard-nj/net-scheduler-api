namespace NetScheduler.Services.Tasks;
using NetScheduler.Models.Events;
using NetScheduler.Models.Tasks;

public static class TaskExtensions
{
    public static ApiEvent ToApiEvent(
        this ScheduleTaskModel task)
    {
        return new ApiEvent
        {
            Endpoint = task.Endpoint,
            Method = task.Method,
            Body = task.Payload
        };
    }
}
