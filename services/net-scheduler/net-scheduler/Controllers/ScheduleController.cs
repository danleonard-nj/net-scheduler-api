namespace NetScheduler.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetScheduler.Configuration.Constants;
using NetScheduler.Models.Schedules;
using NetScheduler.Services.Schedules.Abstractions;
using System.Net;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(
        IScheduleService scheduleService,
        ILogger<ScheduleController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    [HttpGet("{scheduleid}")]
    [Authorize(AuthScheme.Read)]
    public async Task<IActionResult> GetSchedule(string scheduleId, CancellationToken token)
    {
        try
        {
            var schedule = await _scheduleService.GetSchedule(
                scheduleId, 
                token);

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpPost]
    [Authorize(AuthScheme.Write)]
    public async Task<IActionResult> CreateSchedule(
        CreateScheduleModel createScheduleModel,
        CancellationToken token)
    {
        try
        {
            var schedule = await _scheduleService.CreateSchedule(
                createScheduleModel,
                token);

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpDelete("{scheduleId}")]
    [Authorize(AuthScheme.Write)]
    public async Task<IActionResult> DeleteSchedule(string scheduleId, CancellationToken token)
    {
        await _scheduleService.DeleteSchedule(scheduleId, token);

        return Ok(new {id = scheduleId});
    }

    [HttpGet]
    [Authorize(AuthScheme.Read)]
    public async Task<IActionResult> GetSchedules(CancellationToken token)
    {
        try
        {
            var schedules = await _scheduleService.GetSchedules(token);

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpPut]
    [Authorize(AuthScheme.Write)]
    public async Task<IActionResult> UpsertSchedule(ScheduleModel scheduleModel, CancellationToken token)
    {
        var schedule = await _scheduleService.UpsertSchedule(scheduleModel, token);

        return Ok(schedule);
    }

    [HttpGet("Poll")]
    [Authorize(AuthScheme.Execute)]
    public async Task<IActionResult> Poll(CancellationToken token)
    {
        try
        {
            var results = await _scheduleService.Poll(
                token);

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpPost("{scheduleId}/Run")]
    [Authorize(AuthScheme.Execute)]
    public async Task<IActionResult> RunSchedule(string scheduleId, CancellationToken token)
    {
        try
        {
            await _scheduleService.RunSchedule(
                scheduleId,
                token);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }
}
