using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using DeepNoteWindows.ViewModels;
using DeepNoteWindows.Models;

namespace DeepNoteWindows.Views;

public sealed partial class TranscriptPage : Page
{
    private readonly TranscriptViewModel _viewModel;

    public TranscriptPage()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetRequiredService<TranscriptViewModel>();
        DataContext = _viewModel;

        ChunksListView.ItemsSource = _viewModel.Chunks;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is TranscriptPageParams navParams)
        {
            // New flow: receives session + optional finalize result
            await _viewModel.SetSessionAsync(navParams.Session);
            UpdateUI();

            if (navParams.FinalizeResult?.Jobs != null)
            {
                await _viewModel.StartPollingJobsAsync(navParams.FinalizeResult.Jobs);
            }
        }
        else if (e.Parameter is Session session)
        {
            // Legacy flow: just a session
            await _viewModel.SetSessionAsync(session);
            UpdateUI();

            // If summary/quiz are still generating, start polling
            if (_viewModel.IsSummaryGenerating || _viewModel.IsQuizGenerating)
            {
                var fakeJobs = new List<JobInfo>();
                if (_viewModel.IsSummaryGenerating)
                    fakeJobs.Add(new JobInfo { Type = "summary", JobId = "poll" });
                if (_viewModel.IsQuizGenerating)
                    fakeJobs.Add(new JobInfo { Type = "quiz", JobId = "poll" });
                await _viewModel.StartPollingJobsAsync(fakeJobs);
            }
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _viewModel.StopPolling();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TranscriptViewModel.IsLoading):
                LoadingRing.IsActive = _viewModel.IsLoading && _viewModel.Chunks.Count == 0;
                LoadMoreRing.IsActive = _viewModel.IsLoading && _viewModel.Chunks.Count > 0;
                break;
            case nameof(TranscriptViewModel.HasMore):
                LoadMoreButton.Visibility = _viewModel.HasMore && !_viewModel.IsLoading
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                break;
            case nameof(TranscriptViewModel.ErrorMessage):
                ShowError(_viewModel.ErrorMessage);
                break;
            case nameof(TranscriptViewModel.LoadedCount):
            case nameof(TranscriptViewModel.TotalCount):
                LoadedCountText.Text = $"{_viewModel.LoadedCount} / {_viewModel.TotalCount} チャンク";
                break;
            case nameof(TranscriptViewModel.SummaryMarkdown):
            case nameof(TranscriptViewModel.IsSummaryGenerating):
            case nameof(TranscriptViewModel.SummaryError):
            case nameof(TranscriptViewModel.SummaryStatus):
                UpdateSummaryUI();
                break;
            case nameof(TranscriptViewModel.QuizMarkdown):
            case nameof(TranscriptViewModel.IsQuizGenerating):
            case nameof(TranscriptViewModel.QuizError):
            case nameof(TranscriptViewModel.QuizStatus):
                UpdateQuizUI();
                break;
        }
    }

    private void UpdateUI()
    {
        TitleText.Text = _viewModel.SessionTitle;
        StatusText.Text = _viewModel.SessionStatus;
        CountText.Text = $"{_viewModel.TotalCount} チャンク";

        UpdateSummaryUI();
        UpdateQuizUI();
    }

    private void UpdateSummaryUI()
    {
        var hasContent = !string.IsNullOrEmpty(_viewModel.SummaryMarkdown);
        var isGenerating = _viewModel.IsSummaryGenerating;
        var hasError = !string.IsNullOrEmpty(_viewModel.SummaryError);

        // Show expander if there's any state to display
        SummaryExpander.Visibility = (hasContent || isGenerating || hasError)
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Toggle sub-panels
        SummaryGeneratingPanel.Visibility = isGenerating ? Visibility.Visible : Visibility.Collapsed;
        SummaryErrorPanel.Visibility = (!isGenerating && hasError) ? Visibility.Visible : Visibility.Collapsed;
        SummaryContentPanel.Visibility = (!isGenerating && hasContent) ? Visibility.Visible : Visibility.Collapsed;

        if (hasContent)
        {
            SummaryText.Text = _viewModel.SummaryMarkdown;
        }
        if (hasError)
        {
            SummaryErrorText.Text = _viewModel.SummaryError;
        }

        // Auto-expand when content arrives
        if (hasContent && !SummaryExpander.IsExpanded)
        {
            SummaryExpander.IsExpanded = true;
        }
    }

    private void UpdateQuizUI()
    {
        var hasContent = !string.IsNullOrEmpty(_viewModel.QuizMarkdown);
        var isGenerating = _viewModel.IsQuizGenerating;
        var hasError = !string.IsNullOrEmpty(_viewModel.QuizError);

        QuizExpander.Visibility = (hasContent || isGenerating || hasError)
            ? Visibility.Visible
            : Visibility.Collapsed;

        QuizGeneratingPanel.Visibility = isGenerating ? Visibility.Visible : Visibility.Collapsed;
        QuizErrorPanel.Visibility = (!isGenerating && hasError) ? Visibility.Visible : Visibility.Collapsed;
        QuizContentPanel.Visibility = (!isGenerating && hasContent) ? Visibility.Visible : Visibility.Collapsed;

        if (hasContent)
        {
            QuizText.Text = _viewModel.QuizMarkdown;
        }
        if (hasError)
        {
            QuizErrorText.Text = _viewModel.QuizError;
        }

        if (hasContent && !QuizExpander.IsExpanded)
        {
            QuizExpander.IsExpanded = true;
        }
    }

    private void ShowError(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            ErrorInfoBar.IsOpen = false;
        }
        else
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
        }
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        MainWindow.NavigateTo(typeof(SessionListPage));
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.RefreshSessionCommand.ExecuteAsync(null);
        await _viewModel.ApplyFilterCommand.ExecuteAsync(null);
        UpdateUI();
    }

    private async void OnRetrySummaryClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.GenerateSummaryCommand.ExecuteAsync(null);
    }

    private async void OnRetryQuizClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.GenerateQuizCommand.ExecuteAsync(null);
    }

    private async void OnApplyFilterClick(object sender, RoutedEventArgs e)
    {
        _viewModel.FromMs = (int?)FromMsBox.Value;
        _viewModel.ToMs = (int?)ToMsBox.Value;

        if (double.IsNaN(FromMsBox.Value)) _viewModel.FromMs = null;
        if (double.IsNaN(ToMsBox.Value)) _viewModel.ToMs = null;

        await _viewModel.ApplyFilterCommand.ExecuteAsync(null);
    }

    private async void OnClearFilterClick(object sender, RoutedEventArgs e)
    {
        FromMsBox.Value = double.NaN;
        ToMsBox.Value = double.NaN;
        _viewModel.ClearFilterCommand.Execute(null);
        await _viewModel.ApplyFilterCommand.ExecuteAsync(null);
    }

    private async void OnLoadMoreClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadMoreCommand.ExecuteAsync(null);
    }

    // XAML バインディング用のヘルパーメソッド
    public static string FormatMs(double ms)
    {
        var time = TimeSpan.FromMilliseconds(ms);
        return time.ToString(@"hh\:mm\:ss\.f");
    }

    public static Visibility GetSpeakerVisibility(string? speakerId)
    {
        return string.IsNullOrEmpty(speakerId) ? Visibility.Collapsed : Visibility.Visible;
    }
}
