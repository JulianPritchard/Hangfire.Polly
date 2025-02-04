using Polly;

namespace HangfirePolly;

public class PollyRetry
{
    private Func<Exception, int, Context, Task> _onRetryAsync;
    private int _retryCount;

    public void DoThing()
    {
        var eh = Polly.Policy
            .Handle<Exception>()
            .RetryAsync
            (
                retryCount: _retryCount,
                onRetryAsync: _onRetryAsync
            )
            ;
    }

    private Task Something(Exception exception, int count, Context context)
    {
        //context.

        return Task.CompletedTask;
    }
}