using System;
using System.Threading.Tasks;

namespace ClassnoteX.Core.Interfaces;

public interface IAudioRecorder
{
    bool IsRecording { get; }
    Task InitializeAsync();
    Task StartRecordingAsync(string filePath);
    Task StopRecordingAsync();
    event EventHandler<float> AudioLevelChanged; // For visualization
    event EventHandler<(byte[] Buffer, int BytesRead)> AudioDataAvailable; // For streaming to STT
}
