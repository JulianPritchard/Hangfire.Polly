using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Example.Polly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.LoggingPolicy;
using static PollyReactive.Constants;

namespace PollyReactive;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());

        void LogAction(ILogger? logger, Context context, DelegateResult<HttpResponseMessage> outcome)
        {
            var url = context.GetValueOrDefault(Url)?.ToString() ?? Unknown;
            var happened = outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString();
            logger?.LogInformation("{url}, {happened}", url, happened);
        }

        services.AddHttpClient<FooClient>()
            .AddTransientHttpErrorPolicy(policy => policy.RetryAsync(5))
            .AddTransientHttpErrorPolicy(policy => policy.AsyncLog(ctx => ctx.GetLogger(), LogAction))
            .AddHttpMessageHandler(() => new StubErroringDelegatingHandler());

        var fooClient = services
            .BuildServiceProvider()
            .GetRequiredService<FooClient>();

        var result = await fooClient.GetStringAsync("https://google.com", CancellationToken.None);

        Console.WriteLine($"Got result: {result}");

        Console.ReadKey();
    }
}