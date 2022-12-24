namespace NetScheduler.Services.Tasks.Extensions;
using NetScheduler.Models.Events;
using NetScheduler.Models.Tasks;

public static class TaskExtensions
{
    public static ApiEvent ToApiEvent(
        this TaskModel task,
        string token)
    {
        var headers = new
        {
            Authorization = $"Bearer {token}"
        };

        return new ApiEvent
        {
            Endpoint = task.Endpoint,
            Method = task.Method,
            Body = task.Payload,
            Headers = headers,
            ClientId = task.IdentityClientId,
            EventKey = task.TaskName
        };
    }
}
