namespace NetScheduler.Controllers;

using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("Alive")]
    public IActionResult Alive() => Ok();

    [HttpGet("Ready")]
    public IActionResult Ready() => Ok();
}
