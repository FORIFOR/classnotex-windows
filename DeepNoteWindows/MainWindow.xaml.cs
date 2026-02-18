using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DeepNoteWindows.Services;
using DeepNoteWindows.Views;

namespace DeepNoteWindows;

public sealed partial class MainWindow : Window
{
    public static Frame? NavigationFrame { get; private set; }

    public MainWindow()
    {
        this.InitializeComponent();
        NavigationFrame = ContentFrame;

        // 認証状態に応じて初期画面を設定
        var authService = App.Services.GetRequiredService<IAuthService>();
        if (authService.IsAuthenticated)
        {
            ContentFrame.Navigate(typeof(SessionListPage));
        }
        else
        {
            ContentFrame.Navigate(typeof(LoginPage));
        }
    }

    public static void NavigateTo(Type pageType, object? parameter = null)
    {
        NavigationFrame?.Navigate(pageType, parameter);
    }

    public static void GoBack()
    {
        if (NavigationFrame?.CanGoBack == true)
        {
            NavigationFrame.GoBack();
        }
    }
}
