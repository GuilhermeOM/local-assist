namespace Local.Assist.Domain.Services;

public interface ISpeechService
{
    SemaphoreSlim SpeechInProgress { get; }

    Task SpeechAsync(
        string text,
        CancellationToken cancellationToken = default);
}
