namespace Local.Assist.Infrastructure.Resiliences;

internal static class ResilienceNames
{
    public static string HttpClientRetryPolicy(string httpClientName) => $"httpclient-{httpClientName}-retry-policy";
    public static string HttpClientCircuitBreakerPolicy(string httpClientName) => $"httpclient-{httpClientName}-circuitbreaker-policy";
}
