using Local.Assist.Domain.Services;
using Local.Assist.Infrastructure.Backgrounds;
using Local.Assist.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
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
            .AddServices();
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

        var modelName = $"ggml-{modelConfig}";

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
}
