using Hangfire.Polly.Example.Utils;
using Hangfire.Server;

namespace Hangfire.Polly.Example.Services;

public class TestService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<TestService> _logger;

    public TestService(IBackgroundJobClient jobClient, ILogger<TestService> logger)
    {
        _jobClient = jobClient;
        _logger = logger;
    }

    public void Start()
    {
        _jobClient.Schedule(() => DoThing(null), TimeSpan.FromSeconds(5));
    }

    [AutomaticRetry(Attempts = 6, DelaysInSeconds = [1, 2, 3, 5, 8, 13])]
    public void DoThing(PerformContext? context = null)
    {
        var retryCount = context.GetRetryCount();
        _logger.LogInformation("I am doing with with retry count of '{RetryCount}'", retryCount);
        throw new Exception("Fake Exception");
    }
}