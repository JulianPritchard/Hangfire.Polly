using Hangfire.Polly.Example.Services;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable ConvertToPrimaryConstructor

namespace Hangfire.Polly.Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly TestService _testService;

    public TestController(TestService testService)
    {
        _testService = testService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _testService.Start();

        return Ok();
    }
}