using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Local.Assist.Domain.Services;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace Local.Assist.Infrastructure.Services;

internal sealed class MessageService : IMessageService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger<MessageService> _logger;
    private readonly ISpeechService _speechService;
    private readonly Chat _chat;

    public MessageService(
        ILogger<MessageService> logger,
        ISpeechService speechService,
        OllamaApiClient ollamaApiClient)
    {
        _logger = logger;
        _speechService = speechService;
        _chat = new Chat(ollamaApiClient);

        _chat.OnThink += (_, thought) => _logger.LogInformation("[THOUGHT] {Thought}", thought);
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken = default)
    {
        var fullResponse = new StringBuilder();

        var prompt = new Prompt(UserMessage: message);
        var promptString = JsonSerializer.Serialize(prompt, _jsonSerializerOptions);

        await foreach (var response in _chat.SendAsync(promptString, cancellationToken))
        {
            if (string.IsNullOrEmpty(response))
            {
                continue;
            }

            fullResponse.Append(response);
        }

        await _speechService.SpeechAsync(fullResponse.ToString(), cancellationToken);
    }

    private sealed record Prompt(
        [property: JsonPropertyName("personalidade")] string Personalidade = "Você é um assistente de voz. Responda em português brasileiro, com clareza e de forma breve, usando linguagem natural para ser falada em voz alta.",
        [property: JsonPropertyName("observacao")] string Observacao = "Se a mensagem estiver incompleta, confusa ou parecer conter um erro de reconhecimento de fala, peça uma clarificação. Não invente fatos nem diga que realizou uma ação sem ter recebido e executado uma ferramenta apropriada.",
        [property: JsonPropertyName("mensagem_usuario")] string UserMessage = "");
}

