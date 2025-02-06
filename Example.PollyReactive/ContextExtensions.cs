using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Polly.Contrib.LoggingPolicy;

public static class ContextExtensions
{
    private const string LoggerKey = $"{nameof(LoggingPolicy)}.Logger";

    public static Context WithLogger(this Context context, ILogger logger)
    {
        context[LoggerKey] = logger;
        return context;
    }

    public static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue(LoggerKey, out var logger))
        {
            return logger as ILogger;
        }

        return null;
    }
}