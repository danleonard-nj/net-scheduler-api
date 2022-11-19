namespace NetScheduler.Models.Events;

using Azure.Messaging.ServiceBus;
using NetScheduler.Models.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ApiEvent
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("json")]
    public object? Json { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("headers")]
    public AuthorizationHeaders Headers { get; set; }

    public string EventKey { get; set; }

    public string TaskId { get; set; }

    public string ScheduleId { get; set; }

    public ServiceBusMessage ToServiceBusMessage()
    {
        var content = JsonSerializer.SerializeToUtf8Bytes(this);
        return new ServiceBusMessage(content);
    }
}
