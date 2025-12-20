using Microsoft.UI.Xaml;

namespace ClassnoteX.WinUI;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
        CurrentWindow = m_window;
        ServiceLocator.Initialize();
    }

    public static Window CurrentWindow { get; private set; }
    private Window m_window;
}
