using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepNoteWindows.Models;
using DeepNoteWindows.Services;
using System.Collections.ObjectModel;

namespace DeepNoteWindows.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAudioRecorderService _audioRecorder;
    private readonly ISherpaOnnxService _sherpaOnnx;

    private Session? _session;
    private double _currentTimeMs = 0;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isModelLoaded;

    [ObservableProperty]
    private string _sessionTitle = string.Empty;

    [ObservableProperty]
    private string _elapsedTimeText = "00:00:00";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string _manualInputText = string.Empty;

    [ObservableProperty]
    private bool _generateSummary = true;

    [ObservableProperty]
    private bool _generatePlaylist = true;

    [ObservableProperty]
    private bool _generateQuiz = false;

    [ObservableProperty]
    private int _chunkCount = 0;

    public ObservableCollection<TranscriptChunkDisplay> Chunks { get; } = new();

    public RecordingViewModel(
        IApiService apiService,
        IAudioRecorderService audioRecorder,
        ISherpaOnnxService sherpaOnnx)
    {
        _apiService = apiService;
        _audioRecorder = audioRecorder;
        _sherpaOnnx = sherpaOnnx;

        _audioRecorder.AudioBufferReady += OnAudioBufferReady;
        _audioRecorder.RecordingStarted += (s, e) => IsRecording = true;
        _audioRecorder.RecordingStopped += (s, e) => IsRecording = false;
    }

    public void SetSession(Session session)
    {
        _session = session;
        SessionTitle = session.Title;
        _currentTimeMs = 0;
        Chunks.Clear();
        ChunkCount = 0;
    }

    [RelayCommand]
    public async Task LoadModelAsync()
    {
        if (IsModelLoaded) return;

        IsProcessing = true;
        StatusMessage = "音声認識モデルを読み込み中...";
        ErrorMessage = null;

        try
        {
            var modelPath = "models/sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01";
            IsModelLoaded = await _sherpaOnnx.LoadModelAsync(modelPath);

            if (!IsModelLoaded)
            {
                ErrorMessage = "モデルの読み込みに失敗しました。モデルファイルを確認してください。";
            }
            else
            {
                StatusMessage = "モデル読み込み完了";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"モデル読み込みエラー: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    public async Task StartRecordingAsync()
    {
        if (IsRecording || _session == null) return;

        // モデルがロードされていなければロード
        if (!IsModelLoaded)
        {
            await LoadModelAsync();
            if (!IsModelLoaded) return;
        }

        ErrorMessage = null;
        StatusMessage = "録音開始...";

        var success = await _audioRecorder.StartRecordingAsync();
        if (!success)
        {
            ErrorMessage = "録音の開始に失敗しました。マイクの設定を確認してください。";
        }
    }

    [RelayCommand]
    public async Task StopRecordingAsync()
    {
        if (!IsRecording) return;

        await _audioRecorder.StopRecordingAsync();
        StatusMessage = "録音停止";
    }

    private async void OnAudioBufferReady(object? sender, AudioBufferEventArgs e)
    {
        if (!IsRecording || _session == null) return;

        // 経過時間を更新
        UpdateElapsedTime(e.EndTime);

        IsProcessing = true;
        StatusMessage = "文字起こし中...";

        try
        {
            // sherpa-onnx で文字起こし
            var text = await _sherpaOnnx.TranscribeAsync(e.Samples, e.SampleRate);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var chunk = new ChunkInput
                {
                    StartMs = e.StartTime.TotalMilliseconds,
                    EndMs = e.EndTime.TotalMilliseconds,
                    Text = text,
                    Kind = "final"
                };

                // API に送信
                var response = await _apiService.AppendChunksAsync(_session.Id, new List<ChunkInput> { chunk });
                ChunkCount += response.Count;

                // UI に追加
                DispatcherQueue.TryEnqueue(() =>
                {
                    Chunks.Add(new TranscriptChunkDisplay
                    {
                        StartTime = FormatTime(e.StartTime),
                        EndTime = FormatTime(e.EndTime),
                        Text = text
                    });
                });

                StatusMessage = $"チャンク送信完了 ({ChunkCount}件)";
            }
            else
            {
                StatusMessage = "音声が検出されませんでした";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"送信エラー: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue =>
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    private void UpdateElapsedTime(TimeSpan time)
    {
        _currentTimeMs = time.TotalMilliseconds;
        ElapsedTimeText = FormatTime(time);
    }

    private string FormatTime(TimeSpan time)
    {
        return time.ToString(@"hh\:mm\:ss");
    }

    [RelayCommand]
    public async Task SendManualTextAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualInputText) || _session == null) return;

        IsProcessing = true;
        ErrorMessage = null;

        try
        {
            var chunk = new ChunkInput
            {
                StartMs = _currentTimeMs,
                EndMs = _currentTimeMs + 1000, // 仮の終了時間
                Text = ManualInputText.Trim(),
                Kind = "final"
            };

            var response = await _apiService.AppendChunksAsync(_session.Id, new List<ChunkInput> { chunk });
            ChunkCount += response.Count;

            Chunks.Add(new TranscriptChunkDisplay
            {
                StartTime = FormatTime(TimeSpan.FromMilliseconds(_currentTimeMs)),
                EndTime = "--:--:--",
                Text = ManualInputText.Trim()
            });

            ManualInputText = string.Empty;
            _currentTimeMs += 1000;

            StatusMessage = $"テキスト送信完了 ({ChunkCount}件)";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"送信エラー: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    public async Task<FinalizeResponse?> FinalizeSessionAsync()
    {
        if (_session == null) return null;

        // 録音中なら停止
        if (IsRecording)
        {
            await StopRecordingAsync();
        }

        IsProcessing = true;
        StatusMessage = "セッションを完了中...";
        ErrorMessage = null;

        try
        {
            var response = await _apiService.FinalizeAsync(
                _session.Id,
                GenerateSummary,
                GeneratePlaylist,
                GenerateQuiz);

            StatusMessage = "セッション完了！";
            return response;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"完了処理エラー: {ex.Message}";
            return null;
        }
        finally
        {
            IsProcessing = false;
        }
    }
}

public class TranscriptChunkDisplay
{
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
