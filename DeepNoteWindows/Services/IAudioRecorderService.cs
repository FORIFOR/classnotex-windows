namespace DeepNoteWindows.Services;

public interface IAudioRecorderService
{
    bool IsRecording { get; }
    TimeSpan ElapsedTime { get; }

    event EventHandler<AudioBufferEventArgs>? AudioBufferReady;
    event EventHandler? RecordingStarted;
    event EventHandler? RecordingStopped;

    Task<bool> StartRecordingAsync();
    Task StopRecordingAsync();
    List<string> GetAvailableDevices();
    void SetDevice(int deviceIndex);
}

public class AudioBufferEventArgs : EventArgs
{
    public float[] Samples { get; }
    public int SampleRate { get; }
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }

    public AudioBufferEventArgs(float[] samples, int sampleRate, TimeSpan startTime, TimeSpan endTime)
    {
        Samples = samples;
        SampleRate = sampleRate;
        StartTime = startTime;
        EndTime = endTime;
    }
}
