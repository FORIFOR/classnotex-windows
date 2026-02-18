using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using DeepNoteWindows.Services;
using DeepNoteWindows.ViewModels;

namespace DeepNoteWindows;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private Window? _mainWindow;

    public App()
    {
        this.InitializeComponent();

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IAudioRecorderService, AudioRecorderService>();
        services.AddSingleton<ISherpaOnnxService, SherpaOnnxService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SessionListViewModel>();
        services.AddTransient<RecordingViewModel>();
        services.AddTransient<TranscriptViewModel>();

        // HttpClient
        services.AddHttpClient();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}
