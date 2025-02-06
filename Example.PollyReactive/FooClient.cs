using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.LoggingPolicy;
using static PollyReactive.Constants;

namespace PollyReactive;

public class FooClient
{
    private readonly ILogger<FooClient> _logger;
    private readonly HttpClient _client;

    public FooClient(ILogger<FooClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<string> GetStringAsync(string url, CancellationToken token)
    {
        var context = new Context { [Url] = url }.WithLogger(_logger);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.SetPolicyExecutionContext(context);

        var response = await _client.SendAsync(request, token);

        return await response.Content.ReadAsStringAsync(token);
    }
}