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

        foreach (var batch in apiEvents.Chunk(10))
        {
            await SendBatchMessagesAsync(batch);
        }
    }

    public async Task DispatchScheduleHistoryEventAsync(
        IEnumerable<ScheduleHistoryModel> scheduleHistoryModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scheduleHistoryModel, nameof(scheduleHistoryModel));

        // Get the headers for the event to post back to the service
        var authHeaders = await _identityService.GetAuthorizationHeadersAsync(
            _eventConfiguration.ApplicationScope,
            cancellationToken);

        // Create the event to store the scheduler history data
        var apiEvents = scheduleHistoryModel.Select(s => new CreateScheduleHistoryEvent(
            _eventConfiguration.ApplicationBaseUrl,
            authHeaders,
            s));

        _logger.LogInformation(
            "{@Method}: {@HistoryRequests}: Create schedule history events created for dispatch",
            Caller.GetName(),
            scheduleHistoryModel?.Count());

        //var apiEvent = new CreateScheduleHistoryEvent(
        //    _eventConfiguration.ApplicationBaseUrl,
        //    authHeaders,
        //    scheduleHistoryModel);

        _logger.LogInformation(
            "{@Method}: {@HistoryRequests}: Getting service bus queue sender",
            Caller.GetName(),
            scheduleHistoryModel?.Count());

        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        var chunk = 1;
        foreach (var batch in apiEvents.Chunk(10))
        {
            _logger.LogInformation(
               "{@Method}: {@BatchNumber}: Sending message batch",
               Caller.GetName(),
               chunk);

            // Create a batch for the messages
            var messageBatch = await sender.CreateMessageBatchAsync(
                cancellationToken);

            foreach (var message in batch)
            {
                // If we fail to add the message to the batch
                if (!messageBatch.TryAddMessage(message.ToServiceBusMessage()))
                {
                    _logger.LogWarning(
                        "{@Method}: {@Message}: Failed to add message to batch for scheduler history",
                        Caller.GetName(),
                        message);

                    // Throw so we don't store the incomplete history
                    throw new Exception($"Failed to add message to batch for scheduler history");
                }
            }


            await SendBatchMessagesAsync(batch);
            chunk++;
        }
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
                "{@Method}: {@Endpoint}: {@EventKey}: Adding event to batch",
                Caller.GetName(),
                apiEvent.Endpoint,
                apiEvent.EventKey);

            if (!batch.TryAddMessage(apiEvent.ToServiceBusMessage()))
            {
                _logger.LogInformation(
                    "{@Method}: {@Endpoint}: {@EventKey}: Failed to add message to batch",
                    Caller.GetName(),
                    apiEvent.Endpoint,
                    apiEvent.EventKey);

                throw new EventBatchDispatchException(
                    $"Failed to add message to batch: {JsonSerializer.Serialize(apiEvent)}");
            }
        }

        await sender.SendMessagesAsync(
                batch,
                cancellationToken);
    }
}