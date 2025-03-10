using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Polly;

public class PolicyRetryAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    public string PolicyKey { get; init; }
    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<HangfireRetryPolicy> _retryPolicy;

    internal PolicyRetryAttribute(string policyKey, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        PolicyKey = policyKey;
        _retryPolicy = new Lazy<HangfireRetryPolicy>(GetPolicy);
    }

    internal PolicyRetryAttribute(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _retryPolicy = new Lazy<HangfireRetryPolicy>(GetPolicy);
    }

    public PolicyRetryAttribute()
    {
        // Idk...
    }

    private HangfireRetryPolicy GetPolicy()
    {
        return _serviceProvider.GetRequiredKeyedService<HangfireRetryPolicy>(PolicyKey);
    }

    public void OnStateElection(ElectStateContext context)
    {
        throw new NotImplementedException();
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        throw new NotImplementedException();
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        throw new NotImplementedException();
    }
}