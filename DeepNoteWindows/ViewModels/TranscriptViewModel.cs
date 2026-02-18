using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepNoteWindows.Models;
using DeepNoteWindows.Services;
using System.Collections.ObjectModel;

namespace DeepNoteWindows.ViewModels;

public partial class TranscriptViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    private Session? _session;
    private string? _nextCursor;
    private CancellationTokenSource? _pollingCts;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasMore;

    [ObservableProperty]
    private string _sessionTitle = string.Empty;

    [ObservableProperty]
    private string _sessionStatus = string.Empty;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _loadedCount;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _summaryMarkdown;

    [ObservableProperty]
    private string? _summaryStatus;

    [ObservableProperty]
    private bool _isSummaryGenerating;

    [ObservableProperty]
    private string? _summaryError;

    [ObservableProperty]
    private string? _quizMarkdown;

    [ObservableProperty]
    private string? _quizStatus;

    [ObservableProperty]
    private bool _isQuizGenerating;

    [ObservableProperty]
    private string? _quizError;

    [ObservableProperty]
    private int? _fromMs;

    [ObservableProperty]
    private int? _toMs;

    public ObservableCollection<TranscriptChunk> Chunks { get; } = new();

    public TranscriptViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task SetSessionAsync(Session session)
    {
        _session = session;
        SessionTitle = session.Title;
        SessionStatus = session.Status;
        TotalCount = session.TranscriptChunkCount;

        // Use summaryMarkdown (correct backend field), fallback to legacy "summary"
        SummaryMarkdown = session.SummaryMarkdown ?? session.Summary;
        SummaryStatus = session.SummaryStatus;
        SummaryError = session.SummaryError;
        QuizMarkdown = session.QuizMarkdown;
        QuizStatus = session.QuizStatus;
        QuizError = session.QuizError;

        // Determine generation states
        IsSummaryGenerating = IsActiveStatus(SummaryStatus);
        IsQuizGenerating = IsActiveStatus(QuizStatus);

        _nextCursor = null;
        Chunks.Clear();
        LoadedCount = 0;

        await LoadChunksAsync();
    }

    /// <summary>
    /// Start polling for finalize jobs (called after finalize navigation)
    /// </summary>
    public async Task StartPollingJobsAsync(List<JobInfo>? jobs)
    {
        if (jobs == null || _session == null) return;

        _pollingCts?.Cancel();
        _pollingCts = new CancellationTokenSource();
        var ct = _pollingCts.Token;

        var hasSummaryJob = jobs.Any(j => j.Type == "summary" && j.JobId != null);
        var hasQuizJob = jobs.Any(j => j.Type == "quiz" && j.JobId != null);

        if (hasSummaryJob)
        {
            IsSummaryGenerating = true;
            SummaryStatus = "running";
            _ = PollForSummaryAsync(ct);
        }

        if (hasQuizJob)
        {
            IsQuizGenerating = true;
            QuizStatus = "running";
            _ = PollForQuizAsync(ct);
        }

        await Task.CompletedTask;
    }

    public void StopPolling()
    {
        _pollingCts?.Cancel();
        _pollingCts = null;
    }

    private async Task PollForSummaryAsync(CancellationToken ct)
    {
        if (_session == null) return;

        const int maxAttempts = 60;
        const int intervalMs = 2000;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var artifact = await _apiService.GetSummaryArtifactAsync(_session.Id);
                var status = artifact.Status.ToLowerInvariant();

                SummaryStatus = status;

                if (status is "completed" or "ready")
                {
                    SummaryMarkdown = artifact.Result?.Markdown;
                    IsSummaryGenerating = false;
                    SummaryError = null;
                    return;
                }

                if (status is "failed" or "error")
                {
                    SummaryError = artifact.ErrorReason ?? "要約の生成に失敗しました";
                    IsSummaryGenerating = false;
                    return;
                }
            }
            catch
            {
                // Non-fatal: continue polling
            }

            try
            {
                await Task.Delay(intervalMs, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        // Timeout
        IsSummaryGenerating = false;
        SummaryError = "要約の生成がタイムアウトしました。更新ボタンで再確認してください。";
    }

    private async Task PollForQuizAsync(CancellationToken ct)
    {
        if (_session == null) return;

        const int maxAttempts = 60;
        const int intervalMs = 2000;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var artifact = await _apiService.GetQuizArtifactAsync(_session.Id);
                var status = artifact.Status.ToLowerInvariant();

                QuizStatus = status;

                if (status is "completed" or "ready")
                {
                    QuizMarkdown = artifact.Result?.Markdown;
                    IsQuizGenerating = false;
                    QuizError = null;
                    return;
                }

                if (status is "failed" or "error")
                {
                    QuizError = artifact.ErrorReason ?? "クイズの生成に失敗しました";
                    IsQuizGenerating = false;
                    return;
                }
            }
            catch
            {
                // Non-fatal: continue polling
            }

            try
            {
                await Task.Delay(intervalMs, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        // Timeout
        IsQuizGenerating = false;
        QuizError = "クイズの生成がタイムアウトしました。更新ボタンで再確認してください。";
    }

    [RelayCommand]
    public async Task LoadChunksAsync()
    {
        if (IsLoading || _session == null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await _apiService.GetTranscriptChunksAsync(
                _session.Id,
                FromMs,
                ToMs,
                _nextCursor,
                limit: 50);

            foreach (var chunk in response.Chunks)
            {
                Chunks.Add(chunk);
            }

            TotalCount = response.TotalCount;
            LoadedCount = Chunks.Count;
            HasMore = response.HasMore;
            _nextCursor = response.NextCursor;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"チャンクの取得に失敗しました: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadMoreAsync()
    {
        if (!HasMore || IsLoading) return;
        await LoadChunksAsync();
    }

    [RelayCommand]
    public async Task ApplyFilterAsync()
    {
        _nextCursor = null;
        Chunks.Clear();
        LoadedCount = 0;
        await LoadChunksAsync();
    }

    [RelayCommand]
    public void ClearFilter()
    {
        FromMs = null;
        ToMs = null;
    }

    [RelayCommand]
    public async Task RefreshSessionAsync()
    {
        if (_session == null) return;

        IsLoading = true;
        try
        {
            var updatedSession = await _apiService.GetSessionAsync(_session.Id);
            if (updatedSession != null)
            {
                _session = updatedSession;
                SessionTitle = updatedSession.Title;
                SessionStatus = updatedSession.Status;
                TotalCount = updatedSession.TranscriptChunkCount;

                // Update summary
                var newSummary = updatedSession.SummaryMarkdown ?? updatedSession.Summary;
                if (!string.IsNullOrEmpty(newSummary))
                {
                    SummaryMarkdown = newSummary;
                    SummaryStatus = updatedSession.SummaryStatus;
                    IsSummaryGenerating = false;
                    SummaryError = updatedSession.SummaryError;
                }
                else
                {
                    SummaryStatus = updatedSession.SummaryStatus;
                    IsSummaryGenerating = IsActiveStatus(updatedSession.SummaryStatus);
                }

                // Update quiz
                if (!string.IsNullOrEmpty(updatedSession.QuizMarkdown))
                {
                    QuizMarkdown = updatedSession.QuizMarkdown;
                    QuizStatus = updatedSession.QuizStatus;
                    IsQuizGenerating = false;
                    QuizError = updatedSession.QuizError;
                }
                else
                {
                    QuizStatus = updatedSession.QuizStatus;
                    IsQuizGenerating = IsActiveStatus(updatedSession.QuizStatus);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"セッション情報の取得に失敗しました: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Manually trigger summary generation via POST /sessions/{id}/jobs
    /// </summary>
    [RelayCommand]
    public async Task GenerateSummaryAsync()
    {
        if (_session == null || IsSummaryGenerating) return;

        IsSummaryGenerating = true;
        SummaryStatus = "running";
        SummaryError = null;

        try
        {
            await _apiService.CreateJobAsync(_session.Id, "summary");

            _pollingCts?.Cancel();
            _pollingCts = new CancellationTokenSource();
            _ = PollForSummaryAsync(_pollingCts.Token);
        }
        catch (Exception ex)
        {
            SummaryError = $"要約の生成に失敗しました: {ex.Message}";
            IsSummaryGenerating = false;
        }
    }

    /// <summary>
    /// Manually trigger quiz generation via POST /sessions/{id}/jobs
    /// </summary>
    [RelayCommand]
    public async Task GenerateQuizAsync()
    {
        if (_session == null || IsQuizGenerating) return;

        IsQuizGenerating = true;
        QuizStatus = "running";
        QuizError = null;

        try
        {
            await _apiService.CreateJobAsync(_session.Id, "quiz");

            _pollingCts?.Cancel();
            _pollingCts = new CancellationTokenSource();
            _ = PollForQuizAsync(_pollingCts.Token);
        }
        catch (Exception ex)
        {
            QuizError = $"クイズの生成に失敗しました: {ex.Message}";
            IsQuizGenerating = false;
        }
    }

    public string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(ms);
        return time.ToString(@"hh\:mm\:ss\.f");
    }

    private static bool IsActiveStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        var s = status.ToLowerInvariant();
        return s is "running" or "queued" or "pending";
    }
}
