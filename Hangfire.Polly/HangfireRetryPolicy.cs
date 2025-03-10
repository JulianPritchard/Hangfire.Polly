using Hangfire.States;
using Polly;
using Polly.Utilities;

namespace Hangfire.Polly;

public class HangfireRetryPolicy : AsyncPolicy<IState>
{
    protected override async Task<IState> ImplementationAsync
    (
        Func<Context, CancellationToken, Task<IState>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext
    )
    {
        try
        {
            var result = await action.Invoke(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            if (!ResultPredicates.AnyMatch(result))
            {
                return result;
            }

            // Do something

            return result;
        }
        catch (Exception exception)
        {
            var handledException = ExceptionPredicates.FirstMatchOrDefault(exception);
            if (handledException is null) throw;

            // Do something

            handledException.RethrowWithOriginalStackTraceIfDiffersFrom(exception);
            throw;
        }
    }
}