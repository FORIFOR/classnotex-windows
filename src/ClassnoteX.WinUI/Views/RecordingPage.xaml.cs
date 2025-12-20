using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ClassnoteX.WinUI.ViewModels;

namespace ClassnoteX.WinUI.Views;

public sealed partial class RecordingPage : Page
{
    public RecordingViewModel ViewModel { get; }

    public RecordingPage()
    {
        this.InitializeComponent();
        ViewModel = new RecordingViewModel();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
        // Auto-start if requested could go here
    }
}
