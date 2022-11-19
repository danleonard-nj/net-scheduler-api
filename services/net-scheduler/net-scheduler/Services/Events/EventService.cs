namespace NetScheduler.Services.Events;

using Azure.Core;
using Azure.Messaging.ServiceBus;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Azure;
using NetScheduler.Configuration;
using NetScheduler.Models.Events;
using NetScheduler.Services.Events.Abstractions;

public class EventService : IEventService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        IFlurlClientFactory flurlClientFactory,
        ILogger<EventService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientFactory, nameof(clientFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _client = clientFactory.CreateClient(
            "ApiEvents");
    }

    public async Task Send(ServiceBusMessage message)
    {
        _logger.LogInformation(
            "{@Method}: Dispatching event",
            Caller.GetName());

        var sender = _client.CreateSender(
            "kasa-events");

        // Dispatch task execution event
        await sender.SendMessageAsync(
            message);
    }

    private async Task<object> SendRequest(ApiEvent apiEvent)
    {
        var httpMethod = new HttpMethod(apiEvent.Method);

        if (apiEvent.Json != null)
        {
            return await apiEvent.Endpoint
                .WithHeaders(apiEvent.Headers)
                .SendJsonAsync(httpMethod, apiEvent.Json)
                .ReceiveJson();
        }

        // Handle the task execution event request
        return await apiEvent.Endpoint
                .WithHeaders(apiEvent.Headers)
                .SendAsync(httpMethod)
                .ReceiveJson();
    }

    public async Task<object?> HandleEventAsync(ApiEvent apiEvent)
    {
        _logger.LogInformation(
            "{@Method}: {@ApiEvent}: Sending event request",
            Caller.GetName(),
            apiEvent);

        try
        {
            return await SendRequest(apiEvent);
        }

        catch (FlurlHttpException ex)
        {
            _logger.LogInformation(
                "{@Method}: {@StatusCode}: {@Endpoint}: Event request failed",
                Caller.GetName(),
                ex.StatusCode,
                ex.Call.Request.Url);

            return new {ex.Message};
        }
    }
}