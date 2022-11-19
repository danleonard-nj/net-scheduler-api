namespace NetScheduler.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetScheduler.Models.Tasks;
using NetScheduler.Services.Tasks.Abstractions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _scheduleTaskService;
    private readonly ILogger<TaskController> _logger;

    public TaskController(
        ITaskService scheduleTaskService,
        ILogger<TaskController> logger)
    {
        _scheduleTaskService = scheduleTaskService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(
        CreateTaskModel createTaskModel,
        CancellationToken token)
    {
        try
        {
            var task = await _scheduleTaskService.CreateTask(
                createTaskModel,
                token);

            return Ok(task);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(
        string taskId,
        CancellationToken token)
    {
        try
        {
            await _scheduleTaskService.DeleteTask(taskId, token);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new { ex.Message });
        }
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTask(
        string taskId,
        CancellationToken token)
    {
        try
        {
            var schedule = await _scheduleTaskService.GetTask(
                taskId,
                token);

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new
                {
                    ex.Message
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        CancellationToken token)
    {
        try
        {
            var tasks = await _scheduleTaskService.GetTasks(
                token);

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new
                {
                    ex.Message
                });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSchedule(
        ScheduleTaskModel scheduleTaskModel,
        CancellationToken token)
    {
        try
        {
            var task = await _scheduleTaskService.UpsertTask(
                scheduleTaskModel,
                token);

            return Ok(task);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new
                {
                    ex.Message
                });
        }
    }

    [HttpGet("Token/{appId}")]
    public async Task<IActionResult> GetTokenAsync(
        [FromRoute]string appId)
    {
        try
        {
            var token = await _scheduleTaskService.GetTokenAsync(
                appId);

            return Ok(token);
        }
        catch (Exception ex)
        {
            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                new
                {
                    ex.Message
                });
        }
    }
}
