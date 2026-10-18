namespace Local.Assist.Domain.Services;

public interface ITranscriptionService
{
    void StartRecording();
    void AddAudio(byte[] data, int bytesRecorded);
    bool HasSpeech(byte[] buffer, int bytesRecorded);
    Task<string> StopAndTranscribeAsync();
}
