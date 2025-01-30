using Microsoft.AspNetCore.Mvc;

namespace Hangfire.Polly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [AutomaticRetry(Attempts = 6, DelaysInSeconds = [1, 2, 3, 5, 8])]
    public IActionResult Get()
    {
        return Ok();
    }
}