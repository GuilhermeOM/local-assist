using Local.Assist.Infrastructure.HttpClients.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Serilog;

namespace Local.Assist.Infrastructure.Resiliences;

internal static class ResilienceBuilder
{
    public static ResiliencePipelineBuilder HttpClientRetryPolicy(
        ResiliencePipelineBuilder builder,
        HttpClientConfiguration httpClientConfiguration)
    {
        if (httpClientConfiguration.Resilience.Retry is null) return builder;

        var retryStrategy = new RetryStrategyOptions()
        {
            ShouldHandle = args => ShouldHandleHttpClientError(args.Outcome),
            MaxRetryAttempts = httpClientConfiguration.Resilience.Retry.Count,
            Delay = TimeSpan.FromSeconds(httpClientConfiguration.Resilience.Retry.DelayInSeconds),
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = args =>
            {
                var exception = args.Outcome.Exception;

                Log.Logger.Warning(
                    exception,
                    "Retrying HTTP request for {HttpClientName} due to {ExceptionType}: {ExceptionMessage}",
                    httpClientConfiguration.Name,
                    exception?.GetType().Name,
                    exception?.Message);

                return ValueTask.CompletedTask;
            }
        };

        return builder.AddRetry(retryStrategy);
    }

    public static ResiliencePipelineBuilder HttpClientCircuitBreakerPolicy(
        ResiliencePipelineBuilder builder,
        HttpClientConfiguration httpClientConfiguration)
    {
        if (httpClientConfiguration.Resilience.CircuitBreaker is null) return builder;

        var circuitBreakerStrategy = new CircuitBreakerStrategyOptions()
        {
            ShouldHandle = args => ShouldHandleHttpClientError(args.Outcome),
            FailureRatio = httpClientConfiguration.Resilience.CircuitBreaker.FailureRatio,
            SamplingDuration = TimeSpan.FromSeconds(httpClientConfiguration.Resilience.CircuitBreaker.SamplingDurationInSeconds),
            BreakDuration = TimeSpan.FromSeconds(httpClientConfiguration.Resilience.CircuitBreaker.BreakDurationInSeconds),
            OnClosed = args =>
            {
                Log.Logger.Information("Circuit breaker for {HttpClientName} closed. Resuming normal operation.", httpClientConfiguration.Name);
                return ValueTask.CompletedTask;
            },
            OnOpened = args =>
            {
                Log.Logger.Warning("Circuit breaker for {HttpClientName} opened. Requests will be blocked for a period.", httpClientConfiguration.Name);
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {
                Log.Logger.Information("Circuit breaker for {HttpClientName} is half-open. Testing if the service has recovered.", httpClientConfiguration.Name);
                return ValueTask.CompletedTask;
            }
        };

        return builder.AddCircuitBreaker(circuitBreakerStrategy);
    }

    private static ValueTask<bool> ShouldHandleHttpClientError(Outcome<object> outcome)
    {
        var shouldHandle = outcome.Exception switch
        {
            HttpRequestException or TimeoutException => true,
            TaskCanceledException ex when ex.InnerException is TimeoutException => true,
            _ => false
        };

        return ValueTask.FromResult(shouldHandle);
    }
}
