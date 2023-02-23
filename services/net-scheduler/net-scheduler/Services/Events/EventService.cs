namespace NetScheduler.Services.Events;

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using NetScheduler.Configuration;
using NetScheduler.Configuration.Constants;
using NetScheduler.Configuration.Settings;
using NetScheduler.Models.Events;
using NetScheduler.Models.History;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.Events.Exceptions;
using NetScheduler.Services.Identity.Abstractions;
using System.Text.Json;

public class EventService : IEventService
{
    private readonly IIdentityService _identityService;
    private readonly EventConfiguration _eventConfiguration;
    private readonly ServiceBusClient _client;

    private readonly ILogger<EventService> _logger;

    public EventService(
        IAzureClientFactory<ServiceBusClient> clientFactory,
        IIdentityService identityService,
        EventConfiguration eventConfiguration,
        ILogger<EventService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientFactory, nameof(clientFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventConfiguration, nameof(eventConfiguration));
        ArgumentNullException.ThrowIfNull(identityService, nameof(identityService));

        _client = clientFactory.CreateClient(AzureClientName.ApiEvents);

        _logger = logger;
        _identityService = identityService;
        _eventConfiguration = eventConfiguration;
    }

    public string ApplicationBaseUrl
    {
        get => _eventConfiguration.ApplicationBaseUrl;
    }

    public async Task DispatchEventAsync(
        ApiEvent apiEvent,
        string identityClientId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(apiEvent, nameof(apiEvent));

        if (string.IsNullOrWhiteSpace(identityClientId))
        {
            throw new ArgumentNullException(nameof(identityClientId));
        }

        if (string.IsNullOrWhiteSpace(_eventConfiguration.ApiTriggerQueue))
        {
            _logger.LogError(
               "{@Method}: {@QueueName}: Invalid queue name",
               Caller.GetName(),
               _eventConfiguration.ApiTriggerQueue);

            throw new InvalidEventConfigurationException(
                $"Event configuration trigger queue name cannot be null");
        }

        _logger.LogInformation(
           "{@Method}: {@IdentityClientId}: {@ApiEvent}: Fetching auth headers",
           Caller.GetName(),
           identityClientId,
           apiEvent);

        var authHeaders = await _identityService.GetAuthorizationHeadersAsync(
            identityClientId,
            cancellationToken);

        _logger.LogInformation(
           "{@Method}: {@IdentityClientId}: {@AuthHeaders}: Auth headers",
           Caller.GetName(),
           identityClientId,
           authHeaders);

        apiEvent.WithHeaders(authHeaders);

        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        await sender.SendMessageAsync(
            apiEvent.ToServiceBusMessage());
    }


    public async Task DispatchEventsAsync(
        IEnumerable<ApiEvent> apiEvents,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_eventConfiguration.ApiTriggerQueue))
        {
            _logger.LogError(
               "{@Method}: {@QueueName}: Invalid queue name",
               Caller.GetName(),
               _eventConfiguration.ApiTriggerQueue);

            throw new InvalidEventConfigurationException(
                $"Event configuration trigger queue name cannot be null");
        }

        _logger.LogInformation(
           "{@Method}: {@ApiEventCount}: Dispatching events",
           Caller.GetName(),
           apiEvents.Count());

        var sendEvents = apiEvents
            .Chunk(10)
            .Select(async eventBatch =>
            {
                await SendBatchMessagesAsync(eventBatch);
            });

        await Task.WhenAll(sendEvents);
    }

    public async Task DispatchScheduleHistoryEventAsync(
        ScheduleHistoryModel scheduleHistoryModel,
        CancellationToken cancellationToken = default)
    {
        var authHeaders = await _identityService.GetAuthorizationHeadersAsync(
            _eventConfiguration.ApplicationScope,
            cancellationToken);

        var apiEvent = new CreateScheduleHistoryEvent(
            _eventConfiguration.ApplicationBaseUrl,
            authHeaders,
            scheduleHistoryModel);

        _logger.LogInformation(
            "{@Method}: {@ScheduleId}: {@ScheduleName}: Create schedule history event received",
            Caller.GetName(),
            scheduleHistoryModel.ScheduleId,
            scheduleHistoryModel.ScheduleName);

        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        await sender.SendMessageAsync(
            apiEvent.ToServiceBusMessage(),
            cancellationToken);
    }

    private async Task SendBatchMessagesAsync(
        IEnumerable<ApiEvent> events,
        CancellationToken cancellationToken = default)
    {
        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        var batch = await sender.CreateMessageBatchAsync(
            cancellationToken);

        foreach (var apiEvent in events)
        {
            _logger.LogInformation(
                "{@Method}: {@Json}: Adding event to batch",
                Caller.GetName(),
                apiEvent.GetJson());

            if (!batch.TryAddMessage(apiEvent.ToServiceBusMessage()))
            {
                
                throw new EventBatchDispatchException(
                    $"Failed to add message to batch: {JsonSerializer.Serialize(apiEvent)}");
            }

            await sender.SendMessagesAsync(
                batch,
                cancellationToken);
        }
    }
}