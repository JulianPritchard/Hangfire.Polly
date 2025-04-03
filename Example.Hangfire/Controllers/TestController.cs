using Hangfire.Polly.Example.Services;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable ConvertToPrimaryConstructor

namespace Hangfire.Polly.Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ITestServiceFactory _testFactory;

    public TestController(ITestServiceFactory testFactory)
    {
        _testFactory = testFactory;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var testService = _testFactory.With(null);
        testService.Start();

        return Ok();
    }
}