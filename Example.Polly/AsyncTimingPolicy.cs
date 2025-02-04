using System.Diagnostics;
using Polly;

namespace Example.Polly;

public class AsyncTimingPolicy<TResult> : AsyncPolicy<TResult>
{
    private readonly Func<TimeSpan, Context, Task> _timingPublisher;

    public AsyncTimingPolicy(Func<TimeSpan, Context, Task> timingPublisher)
    {
        _timingPublisher = timingPublisher;
    }

    public static AsyncTimingPolicy<TResult> Create(Func<TimeSpan, Context, Task> timingPublisher)
        => new AsyncTimingPolicy<TResult>(timingPublisher);

    protected override async Task<TResult> ImplementationAsync
    (
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext
    )
    {
        var start = Stopwatch.GetTimestamp();

        try
        {
            return await action.Invoke(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            await _timingPublisher.Invoke(elapsed, context).ConfigureAwait(continueOnCapturedContext);
        }
    }
}