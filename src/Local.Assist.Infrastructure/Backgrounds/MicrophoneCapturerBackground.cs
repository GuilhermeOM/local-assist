using System.Diagnostics;
using Local.Assist.Domain.Constants;
using Local.Assist.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.Alsa;

namespace Local.Assist.Infrastructure.Backgrounds;

internal sealed class MicrophoneCapturerBackground : BackgroundService
{
    private static readonly TimeSpan _silenceTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan _silenceCheckInterval = TimeSpan.FromMilliseconds(100);

    private readonly ILogger<MicrophoneCapturerBackground> _logger;
    private readonly IMessageService _messageService;
    private readonly ISpeechService _speechService;
    private readonly ITranscriptionService _transcriptionService;

    private readonly Lock _transcriptLock;

    private long _lastRecognizedSpeechTimestamp;
    private int _recognizerNeedsReset;

    public MicrophoneCapturerBackground(
        ILogger<MicrophoneCapturerBackground> logger,
        IMessageService messageProcessorService,
        ISpeechService speechService,
        ITranscriptionService transcriptionService)
    {
        _logger = logger;
        _messageService = messageProcessorService;
        _speechService = speechService;
        _transcriptionService = transcriptionService;

        _transcriptLock = new Lock();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var microphone = new AlsaIn("pulse")
        {
            WaveFormat = new WaveFormat(
                AudioConstants.SampleRate,
                AudioConstants.Bits,
                AudioConstants.Channel),
        };

        microphone.DataAvailable += (sender, eventArgs) => OnMicrophoneDataAvailable(sender, eventArgs);
        microphone.StartRecording();

        _transcriptionService.StartRecording();

        _logger.LogInformation("Microphone recording started");

        try
        {
            using var timer = new PeriodicTimer(_silenceCheckInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var transcript = await TakeTranscriptAfterSilence();
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    continue;
                }

                Console.WriteLine(transcript);

                try
                {
                    await _messageService.ProcessAsync(transcript, stoppingToken);
                    _transcriptionService.StartRecording();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to process recognized speech: {Transcript}", transcript);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            microphone.StopRecording();
            _logger.LogInformation("Microphone recording stopped");
        }
    }

    private void OnMicrophoneDataAvailable(object? sender, WaveInEventArgs eventArgs)
    {
        if (!_speechService.SpeechInProgress.Wait(0))
        {
            Interlocked.Exchange(ref _recognizerNeedsReset, 1);
            return;
        }

        try
        {
            if (Interlocked.Exchange(ref _recognizerNeedsReset, 0) == 1)
            {
                _transcriptionService.StartRecording();
            }

            _transcriptionService.AddAudio(eventArgs.Buffer, eventArgs.BytesRecorded);

            if (_transcriptionService.HasSpeech(eventArgs.Buffer, eventArgs.BytesRecorded))
            {
                lock (_transcriptLock)
                {
                    _lastRecognizedSpeechTimestamp = Stopwatch.GetTimestamp();
                }
            }
        }
        catch (Exception exception)
        {
            Interlocked.Exchange(ref _recognizerNeedsReset, 1);
            _logger.LogError(exception, "Failed to process microphone audio");
        }
        finally
        {
            _speechService.SpeechInProgress.Release();
        }
    }

    private async Task<string?> TakeTranscriptAfterSilence()
    {
        lock (_transcriptLock)
        {
            if (_lastRecognizedSpeechTimestamp == 0 || Stopwatch.GetElapsedTime(_lastRecognizedSpeechTimestamp) < _silenceTimeout)
            {
                return null;
            }

            _lastRecognizedSpeechTimestamp = 0;
        }

        return await _transcriptionService.StopAndTranscribeAsync();
    }
}
