using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DeepNoteWindows.ViewModels;
using System.Web;

namespace DeepNoteWindows.Views;

public sealed partial class LoginPage : Page
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        this.InitializeComponent();
        _viewModel = App.Services.GetRequiredService<LoginViewModel>();
        DataContext = _viewModel;
    }

    private async void OnGoogleSignInClick(object sender, RoutedEventArgs e)
    {
        try
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            AuthWebView.Visibility = Visibility.Visible;

            await AuthWebView.EnsureCoreWebView2Async();
            AuthWebView.Source = new Uri(_viewModel.GoogleAuthUrl);
        }
        catch (Exception ex)
        {
            ShowError($"WebView の初期化に失敗しました: {ex.Message}");
            LoginPanel.Visibility = Visibility.Visible;
            AuthWebView.Visibility = Visibility.Collapsed;
        }
    }

    private async void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        var uri = new Uri(args.Uri);

        // OAuth コールバックを検出
        // Google OAuth は認証コードをタイトルに表示する方式も使用
        if (uri.AbsolutePath.Contains("approval") || uri.Query.Contains("code="))
        {
            // URLからコードを抽出
            var query = HttpUtility.ParseQueryString(uri.Query);
            var code = query["code"];

            if (!string.IsNullOrEmpty(code))
            {
                args.Cancel = true;
                await ProcessAuthCodeAsync(code);
                return;
            }
        }

        // エラーチェック
        if (uri.Query.Contains("error="))
        {
            args.Cancel = true;
            var query = HttpUtility.ParseQueryString(uri.Query);
            var error = query["error"];
            ShowError($"認証エラー: {error}");
            ResetToLoginView();
        }
    }

    private async Task ProcessAuthCodeAsync(string authCode)
    {
        AuthWebView.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        StatusText.Text = "認証情報を処理中...";

        var success = await _viewModel.HandleAuthCodeAsync(authCode);

        if (success)
        {
            // セッション一覧画面へ遷移
            MainWindow.NavigateTo(typeof(SessionListPage));
        }
        else
        {
            ShowError(_viewModel.ErrorMessage ?? "認証に失敗しました");
            ResetToLoginView();
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void ResetToLoginView()
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
        AuthWebView.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
    }
}
