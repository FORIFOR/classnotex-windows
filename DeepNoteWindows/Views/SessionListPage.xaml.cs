using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using DeepNoteWindows.ViewModels;
using DeepNoteWindows.Models;

namespace DeepNoteWindows.Views;

public sealed partial class SessionListPage : Page
{
    private readonly SessionListViewModel _viewModel;

    public SessionListPage()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetRequiredService<SessionListViewModel>();
        DataContext = _viewModel;

        SessionsListView.ItemsSource = _viewModel.Sessions;
        UserNameText.Text = _viewModel.UserName;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await _viewModel.LoadSessionsCommand.ExecuteAsync(null);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SessionListViewModel.IsLoading):
                LoadingRing.IsActive = _viewModel.IsLoading;
                break;
            case nameof(SessionListViewModel.ErrorMessage):
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                {
                    ErrorText.Text = _viewModel.ErrorMessage;
                    ErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    ErrorText.Visibility = Visibility.Collapsed;
                }
                break;
        }

        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        var isEmpty = _viewModel.Sessions.Count == 0 && !_viewModel.IsLoading;
        EmptyStatePanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        SessionsListView.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadSessionsCommand.ExecuteAsync(null);
    }

    private async void OnSignOutClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.SignOutCommand.ExecuteAsync(null);
        MainWindow.NavigateTo(typeof(LoginPage));
    }

    private async void OnCreateSessionClick(object sender, RoutedEventArgs e)
    {
        await CreateNewSession();
    }

    private async void OnTitleBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await CreateNewSession();
        }
    }

    private async Task CreateNewSession()
    {
        var title = NewSessionTitleBox.Text.Trim();
        if (string.IsNullOrEmpty(title))
            return;

        await _viewModel.CreateSessionCommand.ExecuteAsync(title);

        if (string.IsNullOrEmpty(_viewModel.ErrorMessage) && _viewModel.Sessions.Count > 0)
        {
            NewSessionTitleBox.Text = string.Empty;

            // 新しく作成したセッションの録音画面へ遷移
            var newSession = _viewModel.Sessions[0];
            MainWindow.NavigateTo(typeof(RecordingPage), newSession);
        }
    }

    private void OnSessionClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Session session)
        {
            // 録音済みならトランスクリプト画面、そうでなければ録音画面へ
            if (session.Status == "録音済み" || session.HasTranscript)
            {
                MainWindow.NavigateTo(typeof(TranscriptPage), session);
            }
            else
            {
                MainWindow.NavigateTo(typeof(RecordingPage), session);
            }
        }
    }
}
