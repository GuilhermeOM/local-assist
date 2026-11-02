using System.Text.Json;

namespace Local.Assist.Domain.ValueObjects;

public sealed record PromptResult
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;

    public static PromptResult Success(string message) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static PromptResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };

    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);
}
