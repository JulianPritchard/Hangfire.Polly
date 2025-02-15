﻿namespace Hangfire.Polly.Utils;

public static class StringEx
{
    public static bool Exists(this string? source)
        => !string.IsNullOrWhiteSpace(source);

    public static string ExistsOr(this string? source, string or)
        => source.Exists()
            ? source!
            : or
    ;
}