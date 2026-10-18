using System.Buffers.Binary;
using Local.Assist.Domain.Constants;
using Local.Assist.Domain.Services;
using Whisper.net;

namespace Local.Assist.Infrastructure.Services;

internal sealed class TranscriptionService : ITranscriptionService, IDisposable
{
    private readonly float _normalizeConst = (float)Math.Pow(2, AudioConstants.Bits) / 2;

    private readonly WhisperFactory _factory;
    private readonly WhisperProcessor _processor;

    private readonly List<float> _samples = [];
    private readonly Lock _sync = new();
    private readonly SemaphoreSlim _inferenceLock = new(1, 1);

    private bool _isRecording;

    public TranscriptionService(string modelPath)
    {
        _factory = WhisperFactory.FromPath(modelPath);

        _processor = _factory
            .CreateBuilder()
            .WithLanguage("pt")
            .Build();
    }

    public void StartRecording()
    {
        lock (_sync)
        {
            _samples.Clear();
            _isRecording = true;
        }
    }

    public void AddAudio(byte[] buffer, int bytesRecorded)
    {
        lock (_sync)
        {
            if (!_isRecording)
            {
                return;
            }

            if (bytesRecorded < 0
                || bytesRecorded > buffer.Length
                || bytesRecorded % 2 != 0)
            {
                throw new ArgumentException("Expected complete PCM16 samples.");
            }

            for (var i = 0; i < bytesRecorded; i += 2)
            {
                var pcm = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(i, 2));
                _samples.Add(pcm / _normalizeConst);
            }
        }
    }

    public bool HasSpeech(byte[] buffer, int bytesRecorded)
    {
        var sampleCount = bytesRecorded / (AudioConstants.Bits / 8);
        if (sampleCount == 0)
        {
            return false;
        }

        var sumSquares = 0d;

        for (var i = 0; i < bytesRecorded; i += 2)
        {
            var sample = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(i, 2));
            var normalized = sample / _normalizeConst;
            sumSquares += normalized * normalized;
        }

        var rms = Math.Sqrt(sumSquares / sampleCount);
        return rms >= AudioConstants.RCMThreshold;
    }

    public async Task<string> StopAndTranscribeAsync()
    {
        float[] audio;

        lock (_sync)
        {
            _isRecording = false;

            // Snapshot the buffer without holding the lock
            // during Whisper inference.
            audio = [.. _samples];
            _samples.Clear();
        }

        if (audio.Length == 0)
        {
            return string.Empty;
        }

        await _inferenceLock.WaitAsync();

        try
        {
            var text = new List<string>();

            await foreach (var result in _processor.ProcessAsync(audio))
            {
                text.Add(result.Text);
            }

            return string.Join(" ", text).Trim();
        }
        finally
        {
            _inferenceLock.Release();
        }
    }

    public void Dispose()
    {
        _processor.Dispose();
        _factory.Dispose();
        _inferenceLock.Dispose();
    }
}
