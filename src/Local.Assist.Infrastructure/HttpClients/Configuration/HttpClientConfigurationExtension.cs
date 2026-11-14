using Local.Assist.Infrastructure.HttpClients.DelegateHandlers;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace Local.Assist.Infrastructure.HttpClients.Configuration;

internal static class HttpClientConfigurationExtension
{
    public static void Configure(this HttpClient httpClient, HttpClientConfiguration configuration)
    {
        const string userAgent = "LocalAssist/1.0";

        httpClient.BaseAddress = new Uri(configuration.BaseAddress);
        httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutInSeconds);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
    }

    public static IHttpClientBuilder AddResilienceHandler<THttpClient>(this IHttpClientBuilder builder) where THttpClient : class
    {
        var httpClientName = typeof(THttpClient).Name;

        return builder.AddHttpMessageHandler(serviceProvider =>
        {
            var resiliencePipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
            var configuration = serviceProvider.GetRequiredKeyedService<HttpClientConfiguration>(httpClientName);

            return new ResilienceDelegateHandler(resiliencePipelineProvider, configuration);
        });
    }

    public static IHttpClientBuilder AddCustomHttpClient<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        return services.AddHttpClient<TInterface, TImplementation>((serviceProvider, httpClient) =>
        {
            var configuration = serviceProvider.GetRequiredKeyedService<HttpClientConfiguration>(typeof(TImplementation).Name);
            httpClient.Configure(configuration);
        })
        .AddResilienceHandler<TImplementation>()
        .AddHttpMessageHandler<LoggingDelegateHandler>();
    }
}
