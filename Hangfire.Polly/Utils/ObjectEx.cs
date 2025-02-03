namespace Hangfire.Polly.Utils;

internal static class ObjectEx
{
    public static bool Exists(this object? source)
        => source is not null;
}