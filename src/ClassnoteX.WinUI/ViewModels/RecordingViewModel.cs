using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.Core.Models;
using ClassnoteX.WinUI.Services;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace ClassnoteX.WinUI.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IAudioRecorder _audioRecorder;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISttService _sttService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    
    public ObservableCollection<TranscriptSegment> LiveTranscript { get; } = new();
    
    [ObservableProperty]
    private string sessionTitle = "New Recording";

    [ObservableProperty]
    private string durationText = "00:00:00";

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private double audioLevel;

    private DateTime _startTime;
    private DispatcherQueueTimer? _timer;

    public RecordingViewModel()
    {
        _audioRecorder = ServiceLocator.GetService<IAudioRecorder>();
        _sessionRepository = ServiceLocator.GetService<ISessionRepository>();
        _navigationService = ServiceLocator.GetService<INavigationService>();
        _sttService = ServiceLocator.GetService<ISttService>();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        
        SessionTitle = $"Session - {DateTime.Now:g}";
        
        _audioRecorder.AudioDataAvailable += OnAudioDataAvailable;
        _sttService.SegmentReceived += OnSegmentReceived;
    }

    private async void OnAudioDataAvailable(object? sender, (byte[] Buffer, int BytesRead) e)
    {
        await _sttService.ProcessAudioBufferAsync(e.Buffer, e.BytesRead);
    }

    private void OnSegmentReceived(object? sender, TranscriptSegment e)
    {
        _dispatcherQueue.TryEnqueue(() => 
        {
            LiveTranscript.Add(e);
        });
    }

    public async Task InitializeAsync()
    {
        await _audioRecorder.InitializeAsync();
        await _sttService.InitializeAsync();
    }

    private string? _currentFilePath;

    [RelayCommand]
    private async Task StartRecording()
    {
        if (IsRecording) return;

        // Path should be in AppData
        var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        var filename = $"recording_{DateTime.Now.Ticks}.wav";
        _currentFilePath = System.IO.Path.Combine(folder, filename);

        await _audioRecorder.StartRecordingAsync(_currentFilePath);
        
        IsRecording = true;
        _startTime = DateTime.Now;
        
        // Start Timer
        _timer = _dispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    [RelayCommand]
    private async Task StopRecording()
    {
        if (!IsRecording) return;

        await _audioRecorder.StopRecordingAsync();
        IsRecording = false;
        _timer?.Stop();
        
        // Create and Save Session
        var session = new Session
        {
            Title = SessionTitle,
            CreatedAt = _startTime,
            DurationSeconds = (DateTime.Now - _startTime).TotalSeconds,
            LocalAudioPath = _currentFilePath,
            Status = SessionStatus.Completed // Marking as completed for MVP
        };
        
        await _sessionRepository.InitializeAsync();
        await _sessionRepository.SaveAsync(session);

        // Go back to dashboard (which will reload)
        _navigationService.GoBack();
    }

    private void Timer_Tick(DispatcherQueueTimer sender, object args)
    {
        var elapsed = DateTime.Now - _startTime;
        DurationText = elapsed.ToString(@"hh\:mm\:ss");
    }
}
