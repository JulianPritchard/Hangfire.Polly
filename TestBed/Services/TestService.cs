using Hangfire;
using Hangfire.Polly;
using Hangfire.Server;

namespace TestBed.Services;

public class TestService
{
    private readonly IBackgroundJobClient _jobClient;
    private static readonly HangfireRetryPolicy RetryPolicy = null!;

    public TestService(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public void Start()
    {
        _jobClient.Schedule(() => DoThing(null), TimeSpan.FromSeconds(5));
    }

    [PolicyRetry(PolicyKey = "Cheese")]
    public void DoThing(PerformContext? context = null)
    {

    }
}