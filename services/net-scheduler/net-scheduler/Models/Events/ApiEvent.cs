namespace NetScheduler.Models.Events;

using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

public class ApiEvent
{
    public string Endpoint { get; set; }

    public object? Body { get; set; }

    public string Method { get; set; }

    public object? Headers { get; set; }

    public string? EventKey { get; set; }

    public string? ClientId { get; set; }

    public void WithHeaders(object? headers)
    {
        Headers = headers;
    }

    public string GetJson()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var content = JsonSerializer.Serialize(
            this,
            options: jsonOptions);

        return content;
    }

    public ServiceBusMessage ToServiceBusMessage()
    {
        var jsonBytes = Encoding.UTF8.GetBytes(GetJson());
        
        return new ServiceBusMessage(jsonBytes);
    }
}