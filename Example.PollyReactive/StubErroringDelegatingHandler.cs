using System.Net;

namespace PollyReactive;

public class StubErroringDelegatingHandler : DelegatingHandler
{
    private readonly Random _random = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
        => _random.Next(3) switch
        {
            1 => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            2 => throw new HttpRequestException(),
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("Dummy content from the stub helper class.") }),
        };
}