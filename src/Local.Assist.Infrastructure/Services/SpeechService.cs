using System.Diagnostics;
using Local.Assist.Domain.Services;

namespace Local.Assist.Infrastructure.Services;

public sealed class SpeechService : ISpeechService
{
    private readonly string _python;
    private readonly string _voiceDataDirectory;
    private readonly string _voiceName;

    public SemaphoreSlim SpeechInProgress { get; }

    public SpeechService(string voiceName = "pt_BR-faber-medium")
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        _python = Path.Combine(
            home,
            ".venvs",
            "piper",
            "bin",
            "python");

        _voiceDataDirectory = Path.Combine(home, "piper-voices");
        _voiceName = voiceName;

        SpeechInProgress = new SemaphoreSlim(1, 1);
    }

    public async Task SpeechAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var wavFile = Path.Combine(Path.GetTempPath(), $"resposta-{Guid.NewGuid():N}.wav");

        try
        {
            await CreateAudioAsync(text, wavFile, cancellationToken);
            await PlayAudioAsync(wavFile, cancellationToken);
        }
        finally
        {
            if (File.Exists(wavFile)) File.Delete(wavFile);
        }
    }

    private async Task CreateAudioAsync(
        string texto,
        string arquivoWav,
        CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = _python,
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        info.ArgumentList.Add("-m");
        info.ArgumentList.Add("piper");

        info.ArgumentList.Add("--data-dir");
        info.ArgumentList.Add(_voiceDataDirectory);

        info.ArgumentList.Add("-m");
        info.ArgumentList.Add(_voiceName);

        info.ArgumentList.Add("-f");
        info.ArgumentList.Add(arquivoWav);

        info.ArgumentList.Add("--");
        info.ArgumentList.Add(texto);

        using var process = Process.Start(info)
            ?? throw new InvalidOperationException("Failure starting Piper.");

        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Piper failed: {error}");
        }
    }

    private async Task PlayAudioAsync(
        string wavFile,
        CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = "aplay",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        info.ArgumentList.Add("-D");
        info.ArgumentList.Add("pulse");

        info.ArgumentList.Add("-q");
        info.ArgumentList.Add(wavFile);

        await SpeechInProgress.WaitAsync(cancellationToken);
        try
        {
            using var process = Process.Start(info)
                ?? throw new InvalidOperationException("Failure starting aplay.");

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Aplay could not play the audio file.");
            }
        }
        finally
        {
            SpeechInProgress.Release();
        }
    }
}
