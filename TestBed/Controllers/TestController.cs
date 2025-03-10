using Microsoft.AspNetCore.Mvc;
using TestBed.Services;

namespace TestBed.Controllers;

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