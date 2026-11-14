using Local.Assist.Domain.HttpClients.Nominatim;
using Local.Assist.Domain.HttpClients.OpenMeteo;
using Local.Assist.Domain.Services;
using Local.Assist.Infrastructure.Backgrounds;
using Local.Assist.Infrastructure.HttpClients.Configuration;
using Local.Assist.Infrastructure.HttpClients.DelegateHandlers;
using Local.Assist.Infrastructure.Resiliences;
using Local.Assist.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using Polly;
using Whisper.net.Ggml;

namespace Local.Assist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddTranscription(configuration)
            .AddOllama(configuration)
            .AddBackgroundServices()
            .AddServices()
            .AddHttpClients(configuration);
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<MicrophoneCapturerBackground>();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ISpeechService, SpeechService>();
        return services;
    }

    private static IServiceCollection AddTranscription(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var modelConfig = configuration.GetSection("STTModel").Value ?? string.Empty;

        if (!Enum.TryParse<GgmlType>(modelConfig, ignoreCase: true, out var model))
        {
             throw new InvalidOperationException("Missing speech to text model configuration.");
        }

        var modelName = $"ggml-{modelConfig}.bin";

        if (!File.Exists(modelName))
        {
            using var modelStream = WhisperGgmlDownloader.Default.GetGgmlModelAsync(model).Result;
            using var fileWriter = File.OpenWrite(modelName);
            modelStream.CopyTo(fileWriter);
        }

        return services.AddSingleton<ITranscriptionService, TranscriptionService>(_ => new TranscriptionService(modelName));
    }

    private static IServiceCollection AddOllama(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var llm = configuration.GetSection("LLModel").Value;
        if (string.IsNullOrWhiteSpace(llm))
        {
            throw new InvalidOperationException("Missing large language model configuration.");
        }

        var ollamaApiClient = new OllamaApiClient("http://localhost:11434", llm);
        return services.AddSingleton(ollamaApiClient);
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<LoggingDelegateHandler>();

        var httpClients = configuration.GetRequiredSection("HttpClients").Get<HttpClientConfiguration[]>();
        if (httpClients is null || httpClients.Length == 0)
        {
            return services;
        }

        foreach (var httpClient in httpClients)
        {
            services.AddKeyedSingleton(httpClient.Name, httpClient);

            services.AddResiliencePipeline(
                ResilienceNames.HttpClientCircuitBreakerPolicy(httpClient.Name),
                builder => ResilienceBuilder.HttpClientCircuitBreakerPolicy(builder, httpClient));

            services.AddResiliencePipeline(
                ResilienceNames.HttpClientRetryPolicy(httpClient.Name),
                builder => ResilienceBuilder.HttpClientRetryPolicy(builder, httpClient));
        }

        services.AddCustomHttpClient<INominatimHttpClient, NominatimHttpClient>();
        services.AddCustomHttpClient<IOpenMeteoHttpClient, OpenMeteoHttpClient>();

        return services;
    }
}
