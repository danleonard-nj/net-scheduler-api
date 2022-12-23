namespace NetScheduler.Services.Events;

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using NetScheduler.Configuration;
using NetScheduler.Configuration.Constants;
using NetScheduler.Configuration.Settings;
using NetScheduler.Models.Events;
using NetScheduler.Models.History;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.Extensions;
using NetScheduler.Services.Identity.Abstractions;

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
        string identityClientId,
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
           "{@Method}: {@IdentityClientId}: {@ApiEventCount}: Fetching auth headers",
           Caller.GetName(),
           identityClientId,
           apiEvents.Count());

        var authHeaders = await _identityService.GetAuthorizationHeadersAsync(
            identityClientId,
            cancellationToken);

        _logger.LogInformation(
           "{@Method}: {@IdentityClientId}: {@AuthHeaders}: Auth headers",
           Caller.GetName(),
           identityClientId,
           authHeaders);

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
            _eventConfiguration.ApplicationScope);

        var apiEvent = new CreateScheduleHistoryEvent(
            _eventConfiguration.ApplicationBaseUrl,
            authHeaders,
            scheduleHistoryModel);

        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        await sender.SendMessageAsync(
            apiEvent.ToServiceBusMessage());
    }

    private async Task SendBatchMessagesAsync(IEnumerable<ApiEvent> events)
    {
        var sender = _client.CreateSender(
            _eventConfiguration.ApiTriggerQueue);

        var batch = await sender.CreateMessageBatchAsync();

        foreach (var apiEvent in events)
        {
            if (!batch.TryAddMessage(apiEvent.ToServiceBusMessage()))
            {
                throw new ServiceBusBatchException(
                    $"Failed to add message to batch: {apiEvent.ToJson()}");
            }

            await sender.SendMessagesAsync(batch);
        }
    }
}