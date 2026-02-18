using NAudio.Wave;
using System.Diagnostics;

namespace DeepNoteWindows.Services;

public class AudioRecorderService : IAudioRecorderService, IDisposable
{
    private WaveInEvent? _waveIn;
    private readonly List<float> _buffer = new();
    private readonly object _bufferLock = new();
    private readonly Stopwatch _stopwatch = new();

    private const int SampleRate = 16000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;
    private const int BufferDurationSeconds = 6;
    private const int SamplesPerBuffer = SampleRate * BufferDurationSeconds;

    private int _deviceIndex = 0;
    private TimeSpan _lastBufferEndTime = TimeSpan.Zero;

    public bool IsRecording { get; private set; }
    public TimeSpan ElapsedTime => _stopwatch.Elapsed;

    public event EventHandler<AudioBufferEventArgs>? AudioBufferReady;
    public event EventHandler? RecordingStarted;
    public event EventHandler? RecordingStopped;

    public List<string> GetAvailableDevices()
    {
        var devices = new List<string>();
        for (int i = 0; i < WaveIn.DeviceCount; i++)
        {
            var caps = WaveIn.GetCapabilities(i);
            devices.Add(caps.ProductName);
        }
        return devices;
    }

    public void SetDevice(int deviceIndex)
    {
        if (IsRecording)
            throw new InvalidOperationException("録音中はデバイスを変更できません");

        _deviceIndex = deviceIndex;
    }

    public Task<bool> StartRecordingAsync()
    {
        if (IsRecording)
            return Task.FromResult(false);

        try
        {
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceIndex,
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                BufferMilliseconds = 100
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            lock (_bufferLock)
            {
                _buffer.Clear();
            }

            _lastBufferEndTime = TimeSpan.Zero;
            _stopwatch.Restart();
            _waveIn.StartRecording();
            IsRecording = true;

            RecordingStarted?.Invoke(this, EventArgs.Empty);
            return Task.FromResult(true);
        }
        catch
        {
            _waveIn?.Dispose();
            _waveIn = null;
            return Task.FromResult(false);
        }
    }

    public Task StopRecordingAsync()
    {
        if (!IsRecording || _waveIn == null)
            return Task.CompletedTask;

        _waveIn.StopRecording();
        _stopwatch.Stop();

        // 残りのバッファを処理
        FlushBuffer();

        return Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // 16bit PCM を float に変換
        var samplesCount = e.BytesRecorded / 2;
        var samples = new float[samplesCount];

        for (int i = 0; i < samplesCount; i++)
        {
            short sample = BitConverter.ToInt16(e.Buffer, i * 2);
            samples[i] = sample / 32768f;
        }

        lock (_bufferLock)
        {
            _buffer.AddRange(samples);

            // 6秒分のバッファが溜まったら処理
            if (_buffer.Count >= SamplesPerBuffer)
            {
                ProcessBuffer();
            }
        }
    }

    private void ProcessBuffer()
    {
        float[] samples;
        TimeSpan startTime;
        TimeSpan endTime;

        lock (_bufferLock)
        {
            if (_buffer.Count == 0)
                return;

            // 6秒分のサンプルを取り出す（またはすべて）
            var takeCount = Math.Min(_buffer.Count, SamplesPerBuffer);
            samples = _buffer.Take(takeCount).ToArray();
            _buffer.RemoveRange(0, takeCount);

            startTime = _lastBufferEndTime;
            endTime = TimeSpan.FromSeconds(samples.Length / (double)SampleRate) + startTime;
            _lastBufferEndTime = endTime;
        }

        AudioBufferReady?.Invoke(this, new AudioBufferEventArgs(
            samples, SampleRate, startTime, endTime));
    }

    private void FlushBuffer()
    {
        lock (_bufferLock)
        {
            if (_buffer.Count > 0)
            {
                var samples = _buffer.ToArray();
                var startTime = _lastBufferEndTime;
                var endTime = TimeSpan.FromSeconds(samples.Length / (double)SampleRate) + startTime;

                _buffer.Clear();

                AudioBufferReady?.Invoke(this, new AudioBufferEventArgs(
                    samples, SampleRate, startTime, endTime));
            }
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        IsRecording = false;
        RecordingStopped?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
        _waveIn = null;
    }
}
