namespace Local.Assist.Infrastructure.HttpClients.Configuration;

internal sealed record HttpClientConfiguration
{
   public string Name { get; init; } = string.Empty;
   public string BaseAddress { get; init; } = string.Empty;
   public int TimeoutInSeconds { get; init; } = 0;
   public HttpClientResilience Resilience { get; init; } = new();
};

internal sealed record HttpClientResilience
{
    public RetryConfigurationResilience? Retry { get; init; }
    public CircuitBreakerConfigurationResilience? CircuitBreaker { get; init; }
}

internal sealed record RetryConfigurationResilience(
    int Count,
    int DelayInSeconds);

internal sealed record CircuitBreakerConfigurationResilience(
    double FailureRatio,
    int SamplingDurationInSeconds,
    int BreakDurationInSeconds);
