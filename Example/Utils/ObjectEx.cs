namespace Hangfire.Polly.Example.Utils;

internal static class ObjectEx
{
    public static bool Exists(this object? source)
        => source is not null;
}