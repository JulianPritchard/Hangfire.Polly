using System.ComponentModel;
using System.Reflection;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;

namespace Hangfire.Polly;
#nullable disable

public class AutomaticPollyRetryAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    private Type[] _onlyOn;
    private Type[] _exceptOn;
    private int _attempts;
    private readonly object _lockObject = new();
    private bool _logEvents;

    private readonly ILog _logger = LogProvider.For<AutomaticRetryAttribute>();

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Type[] OnlyOn
    {
        get { lock (_lockObject) { return _onlyOn; } }
        set { lock (_lockObject) { _onlyOn = value; } }
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Type[] ExceptOn
    {
        get { lock (_lockObject) { return _exceptOn; } }
        set { lock (_lockObject) { _exceptOn = value; } }
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public AttemptsExceededAction OnAttemptsExceeded
    {
        get { lock (_lockObject) { return _onAttemptsExceeded; } }
        set { lock (_lockObject) { _onAttemptsExceeded = value; } }
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    [DefaultValue(true)]
    public bool LogEvents
    {
        get { lock (_lockObject) { return _logEvents; } }
        set { lock (_lockObject) { _logEvents = value; } }
    }

    private AttemptsExceededAction _onAttemptsExceeded;

    public int Attempts
    {
        get { lock (_lockObject) { return _attempts; } }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), @"Attempts value must be equal or greater than zero.");
            }

            lock (_lockObject)
            {
                _attempts = value;
            }
        }
    }

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is not FailedState failedState)
        {
            // This filter accepts only failed job state.
            return;
        }

        if (_onlyOn != null && _onlyOn.Length > 0)
        {
            var exceptionType = failedState.Exception.GetType();
            var satisfied = false;

            foreach (var onlyOn in _onlyOn)
            {
                if (onlyOn.GetTypeInfo().IsAssignableFrom(exceptionType.GetTypeInfo()))
                {
                    satisfied = true;
                    break;
                }
            }

            if (!satisfied) return;
        }

        if (_exceptOn != null && _exceptOn.Length > 0)
        {
            var exceptionType = failedState.Exception.GetType();
            var satisfied = true;

            foreach (var exceptOn in _exceptOn)
            {
                if (exceptOn.GetTypeInfo().IsAssignableFrom(exceptionType.GetTypeInfo()))
                {
                    satisfied = false;
                    break;
                }
            }

            if (!satisfied) return;
        }

        var retryAttempt = context.GetJobParameter<int>("RetryCount", allowStale: true) + 1;

        if (retryAttempt <= Attempts)
        {
            ScheduleAgainLater(context, retryAttempt, failedState);
        }
        else if (retryAttempt > Attempts && OnAttemptsExceeded == AttemptsExceededAction.Delete)
        {
            TransitionToDeleted(context, failedState);
        }
        else
        {
            if (LogEvents)
            {
                _logger.ErrorException(
                    $"Failed to process the job '{context.BackgroundJob.Id}': an exception occurred.",
                    failedState.Exception);
            }
        }
    }

    private void ScheduleAgainLater(ElectStateContext context, int retryAttempt, FailedState failedState)
    {
        throw new NotImplementedException();
    }


    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is ScheduledState &&
            context.NewState.Reason != null &&
            context.NewState.Reason.StartsWith("Retry attempt", StringComparison.OrdinalIgnoreCase))
        {
            transaction.AddToSet("retries", context.BackgroundJob.Id);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (ScheduledState.StateName.Equals(context.OldStateName, StringComparison.OrdinalIgnoreCase))
        {
            transaction.RemoveFromSet("retries", context.BackgroundJob.Id);
        }
    }

    private void TransitionToDeleted(ElectStateContext context, FailedState failedState)
    {
        context.CandidateState = new DeletedState(new ExceptionInfo(failedState.Exception))
        {
            Reason = Attempts > 0
                ? "Exceeded the maximum number of retry attempts."
                : "Retries were disabled for this job."
        };

        if (LogEvents)
        {
            _logger.WarnException(
                $"Failed to process the job '{context.BackgroundJob.Id}': an exception occured. Job was automatically deleted because the retry attempt count exceeded {Attempts}.",
                failedState.Exception);
        }
    }
}
#nullable restore