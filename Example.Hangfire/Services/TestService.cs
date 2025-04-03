using Hangfire.Polly.Example.Utils;
using Hangfire.Server;

namespace Hangfire.Polly.Example.Services;

public class TestService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<TestService> _logger;
    private readonly PerformContext? _context;

    public TestService(IBackgroundJobClient jobClient, ILogger<TestService> logger, PerformContext? context = null)
    {
        _jobClient = jobClient;
        _logger = logger;
        _context = context;
    }

    public void Start()
    {
        _jobClient.Schedule(() => DoThing(), TimeSpan.FromSeconds(5));
    }

    [AutoRetry(Attempts = 6, DelaysInSeconds = [1, 2, 3, 5, 8, 13])]
    public void DoThing()
    {
        var retryCount = _context.GetRetryCount();
        _logger.LogInformation("I am doing with with retry count of '{RetryCount}'", retryCount);
        throw new Exception("Fake Exception");
    }
}