using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using DeepNoteWindows.ViewModels;
using DeepNoteWindows.Models;

namespace DeepNoteWindows.Views;

public sealed partial class RecordingPage : Page
{
    private readonly RecordingViewModel _viewModel;

    public RecordingPage()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetRequiredService<RecordingViewModel>();
        DataContext = _viewModel;

        ChunksListView.ItemsSource = _viewModel.Chunks;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Session session)
        {
            _viewModel.SetSession(session);
            TitleText.Text = session.Title;

            // モデルを事前にロード
            await _viewModel.LoadModelCommand.ExecuteAsync(null);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(RecordingViewModel.IsRecording):
                UpdateRecordingUI();
                break;
            case nameof(RecordingViewModel.IsProcessing):
                ProcessingRing.IsActive = _viewModel.IsProcessing;
                break;
            case nameof(RecordingViewModel.StatusMessage):
                StatusText.Text = _viewModel.StatusMessage ?? string.Empty;
                break;
            case nameof(RecordingViewModel.ErrorMessage):
                ShowError(_viewModel.ErrorMessage);
                break;
            case nameof(RecordingViewModel.ElapsedTimeText):
                ElapsedTimeText.Text = _viewModel.ElapsedTimeText;
                break;
            case nameof(RecordingViewModel.ChunkCount):
                ChunkCountText.Text = _viewModel.ChunkCount.ToString();
                break;
        }
    }

    private void UpdateRecordingUI()
    {
        if (_viewModel.IsRecording)
        {
            RecordButtonText.Text = "録音停止";
            RecordIndicator.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Red);

            // 点滅アニメーション（簡易版）
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += (s, e) =>
            {
                if (!_viewModel.IsRecording)
                {
                    timer.Stop();
                    RecordIndicator.Opacity = 1;
                    return;
                }
                RecordIndicator.Opacity = RecordIndicator.Opacity == 1 ? 0.3 : 1;
            };
            timer.Start();
        }
        else
        {
            RecordButtonText.Text = "録音開始";
            RecordIndicator.Opacity = 1;
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
        MainWindow.GoBack();
    }

    private async void OnRecordClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsRecording)
        {
            await _viewModel.StopRecordingCommand.ExecuteAsync(null);
        }
        else
        {
            await _viewModel.StartRecordingCommand.ExecuteAsync(null);
        }
    }

    private async void OnSendManualTextClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ManualInputText = ManualInputBox.Text;
        await _viewModel.SendManualTextCommand.ExecuteAsync(null);
        ManualInputBox.Text = string.Empty;
    }

    private async void OnManualInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            _viewModel.ManualInputText = ManualInputBox.Text;
            await _viewModel.SendManualTextCommand.ExecuteAsync(null);
            ManualInputBox.Text = string.Empty;
        }
    }

    private async void OnFinalizeClick(object sender, RoutedEventArgs e)
    {
        _viewModel.GenerateSummary = SummaryCheckBox.IsChecked ?? true;
        _viewModel.GeneratePlaylist = PlaylistCheckBox.IsChecked ?? true;
        _viewModel.GenerateQuiz = QuizCheckBox.IsChecked ?? false;

        var response = await _viewModel.FinalizeSessionCommand.ExecuteAsync(null);

        if (response != null)
        {
            // Pass both session info and finalize result to TranscriptPage
            var navParams = new TranscriptPageParams
            {
                Session = new Session
                {
                    Id = response.SessionId,
                    Title = _viewModel.SessionTitle,
                    Status = response.Status,
                    HasTranscript = response.HasTranscript,
                    TranscriptTextLen = response.TranscriptTextLen
                },
                FinalizeResult = response
            };
            MainWindow.NavigateTo(typeof(TranscriptPage), navParams);
        }
    }
}
