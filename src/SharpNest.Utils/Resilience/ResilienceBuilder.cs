using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace SharpNest.Utils.Resilience;

public static class ResilienceBuilder
{
    public static ResiliencePipelineBuilder AddRetryStrategy(this ResiliencePipelineBuilder builder, Func<PredicateBuilder, PredicateBuilder> configurePredicates)
    {
        var predicateBuilder = new PredicateBuilder();
        predicateBuilder = configurePredicates(predicateBuilder);

        var options = new RetryStrategyOptions
        {
            ShouldHandle = predicateBuilder,
            Delay = TimeSpan.FromSeconds(2),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        };

        return builder.AddRetry(options);
    }

    public static ResiliencePipelineBuilder AddRetryStrategy(this ResiliencePipelineBuilder builder, Func<PredicateBuilder, PredicateBuilder> configurePredicates, Action<OnRetryArguments<object>> onRetryAction)
    {
        var predicateBuilder = new PredicateBuilder();
        predicateBuilder = configurePredicates(predicateBuilder);

        var options = new RetryStrategyOptions
        {
            ShouldHandle = predicateBuilder,
            Delay = TimeSpan.FromSeconds(2),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                onRetryAction(args);
                return ValueTask.CompletedTask;
            }
        };

        return builder.AddRetry(options);
    }

    public static ResiliencePipelineBuilder AddCircuitBreakerStrategy(this ResiliencePipelineBuilder builder, ILogger logger, Func<PredicateBuilder, PredicateBuilder> configurePredicates)
    {
        var predicateBuilder = new PredicateBuilder();
        predicateBuilder = configurePredicates(predicateBuilder);

        var options = new CircuitBreakerStrategyOptions
        {
            ShouldHandle = predicateBuilder,
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15),
            OnOpened = args =>
            {
                logger.LogWarning(CircuitBreakerLogMessages.Opened, DateTime.UtcNow.Add(TimeSpan.FromSeconds(15)));
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                logger.LogInformation(CircuitBreakerLogMessages.Closed);
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                logger.LogInformation(CircuitBreakerLogMessages.HalfOpened);
                return ValueTask.CompletedTask;
            }
        };

        return builder.AddCircuitBreaker(options);
    }

    public static ResiliencePipelineBuilder AddTimeoutStrategy(this ResiliencePipelineBuilder builder)
    {
        return builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(30)
        });
    }

    private static class CircuitBreakerLogMessages
    {
        public const string Opened = "Circuit breaker opened. Further requests will fail fast until {ResumeTime}.";
        public const string Closed = "Circuit breaker closed. Requests will flow normally.";
        public const string HalfOpened = "Circuit breaker half-opened. Testing with limited requests.";
    }
}
