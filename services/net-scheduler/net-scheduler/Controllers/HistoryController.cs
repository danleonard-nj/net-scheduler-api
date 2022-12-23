namespace NetScheduler.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetScheduler.Configuration.Constants;
using NetScheduler.Models.History;
using NetScheduler.Models.Schedules;
using NetScheduler.Services.History.Abstractions;
using System.Net;

[Route("api/[controller]")]
[ApiController]
public class HistoryController : ControllerBase
{
    private readonly IScheduleHistoryService _scheduleHistoryService;

    private readonly ILogger<HistoryController> _logger;

	public HistoryController(
        IScheduleHistoryService scheduleHistoryService,
        ILogger<HistoryController> logger)
	{
        ArgumentNullException.ThrowIfNull(scheduleHistoryService, nameof(scheduleHistoryService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _scheduleHistoryService = scheduleHistoryService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(AuthScheme.Execute)]
    public async Task<IActionResult> CreateHistoryEntry(
        ScheduleHistoryModel scheduleHistoryModel,
        CancellationToken token)
	{
        try
        {
            var historyEntry = await _scheduleHistoryService.CreateHistoryEntryAsync(
                scheduleHistoryModel,
                token);

            return Ok(historyEntry);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpGet]
    [Authorize(AuthScheme.Read)]
    public async Task<IActionResult> GetScheduleEntriesAsync(
        [FromQuery] int startTimestamp,
        [FromQuery] int endTimestamp = default,
        CancellationToken token = default)
    {
        try
        {
            var historyEntry = await _scheduleHistoryService.GetHistoryByCreatedDateRangeAsync(
                startTimestamp,
                endTimestamp,
                token);

            return Ok(historyEntry);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }
}
