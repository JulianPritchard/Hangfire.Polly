using Polly;
using Polly.Utilities;
using Microsoft.Extensions.Logging;

namespace Example.Polly;

public class AsyncLoggingPolicy<TResult> : AsyncPolicy<TResult>
{
    private Func<Context, ILogger?> LoggerProvider { get; }
    private Action<ILogger, Context, DelegateResult<TResult>> LogAction { get; }

    internal AsyncLoggingPolicy
    (
        PolicyBuilder<TResult> policyBuilder, // [i]
        Func<Context, ILogger?> loggerProvider,
        Action<ILogger, Context, DelegateResult<TResult>> logAction
    )
    : base(policyBuilder)  // [ii]
    {
        LoggerProvider = loggerProvider ?? throw new NullReferenceException(nameof(loggerProvider));
        LogAction = logAction ?? throw new NullReferenceException(nameof(logAction));
    }

    protected override async Task<TResult> ImplementationAsync
    (
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext
    )
    {
        try
        {
            // [1]
            var result = await action.Invoke(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            if (!ResultPredicates.AnyMatch(result))
            {
                return result;
            }

            // [3]
            // Logic to handle the results
            var logger = LoggerProvider(context);
            LogAction.Invoke(logger, context, new DelegateResult<TResult>(result));

            // [5]
            // Return logic (return result ^ substitute result ^ throw exception)
            return result;
        }
        catch (Exception exception)
        {
            // [2]
            var handledException = ExceptionPredicates.FirstMatchOrDefault(exception);
            if (handledException is null) throw;

            // [4]
            // Custom Exception Handling
            var logger = LoggerProvider(context);
            LogAction.Invoke(logger, context, new DelegateResult<TResult>(exception));

            // [6]
            // Return
            handledException.RethrowWithOriginalStackTraceIfDiffersFrom(exception);
            throw;
        }
    }
}

public static class AsyncLoggingPolicyEx
{
    public static AsyncLoggingPolicy<TResult> AsyncLog<TResult>
    (
        this PolicyBuilder<TResult> policyBuilder,
        Func<Context, ILogger?> loggerProvider,
        Action<ILogger, Context, DelegateResult<TResult>> logAction
    )
    {
        return new AsyncLoggingPolicy<TResult>
        (
            policyBuilder, loggerProvider, logAction
        );
    }
}