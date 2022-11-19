namespace NetScheduler.Controllers;
using Microsoft.AspNetCore.Mvc;
using NetScheduler.Models.Events;
using NetScheduler.Services.Events.Abstractions;

[Route("api/[controller]")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        ArgumentNullException.ThrowIfNull(eventService, nameof(eventService));

        _eventService = eventService;
    }

    [HttpPost]
    public async Task<IActionResult> HandleEvent(ApiEvent apiEvent)
    {
        var result = await _eventService.HandleEventAsync(
            apiEvent);

        return Ok(result);
    }
}
