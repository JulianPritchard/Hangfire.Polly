using Hangfire.Server;

namespace Hangfire.Polly.Utils;

public static class PerformContextEx
{
    public static bool TryGet<T>(this PerformContext? context, string name, out T? result)
    {
        if (context is null)
        {
            result = default;
            return false;
        }

        result = context.GetJobParameter<T>(name);
        return true;
    }

    public static bool TryGetRetryCount(this PerformContext? context, out int count)
        => context.TryGet("RetryCount", out count);

    public static int GetRetryCount(this PerformContext? context)
    {
        context.TryGetRetryCount(out var count);
        return count;
    }
}