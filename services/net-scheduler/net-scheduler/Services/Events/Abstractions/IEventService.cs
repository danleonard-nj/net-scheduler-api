namespace NetScheduler.Services.Events.Abstractions;

using Azure.Messaging.ServiceBus;
using NetScheduler.Models.Events;
using System.Threading.Tasks;

public interface IEventService
{
    Task Send(ServiceBusMessage message);

    Task<object?> HandleEventAsync(ApiEvent apiEvent);
}