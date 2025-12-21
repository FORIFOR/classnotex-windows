using Microsoft.UI.Xaml;
using ClassnoteX.WinUI.Services;

namespace ClassnoteX.WinUI;

public partial class App : Application
{
    private Window? m_window;
    
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

    public static Window? CurrentWindow { get; private set; }
}

