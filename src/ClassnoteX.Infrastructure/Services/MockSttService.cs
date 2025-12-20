using System;
using System.Threading.Tasks;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.Core.Models;

namespace ClassnoteX.Infrastructure.Services;

/// <summary>
/// A placeholder implementation that simulates STT for development environments
/// where Sherpa-ONNX native libraries might be missing (e.g., macOS/CI).
/// </summary>
public class MockSttService : ISttService
{
    public event EventHandler<TranscriptSegment>? SegmentReceived;

    public async Task InitializeAsync()
    {
        // Simulate loading models
        await Task.Delay(1000); 
    }

    private int _counter = 0;

    public async Task ProcessAudioBufferAsync(byte[] buffer, int bytesRead)
    {
        // Simulate occasional output
        if (_counter++ % 50 == 0) // Roughly every 50 calls (approx 1-2 sec depending on buffer size)
        {
            var segment = new TranscriptSegment
            {
                Text = $"Simulated transcription text segment {_counter}",
                StartSeconds = _counter / 10.0,
                EndSeconds = (_counter / 10.0) + 2.0,
                SpeakerId = "0",
                IsFinal = true
            };
            
            SegmentReceived?.Invoke(this, segment);
        }
        await Task.CompletedTask;
    }
}
