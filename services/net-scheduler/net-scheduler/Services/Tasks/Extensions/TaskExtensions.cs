namespace NetScheduler.Services.Tasks.Extensions;
using NetScheduler.Models.Events;
using NetScheduler.Models.Tasks;

public static class TaskExtensions
{
    public static ApiEvent ToApiEvent(
        this TaskModel task)
    {
        return new ApiEvent
        {
            Endpoint = task.Endpoint,
            Method = task.Method,
            Body = task.Payload
        };
    }
}
