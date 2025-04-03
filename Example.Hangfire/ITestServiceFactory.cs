using Hangfire.Polly.Example.Services;
using Hangfire.Server;

namespace Hangfire.Polly.Example;

public interface ITestServiceFactory
{
    TestService With(PerformContext? performContext);
}