using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ClassnoteX.WinUI.ViewModels;

namespace ClassnoteX.WinUI.Views;

public sealed partial class SessionDetailPage : Page
{
    public SessionDetailViewModel ViewModel { get; }

    public SessionDetailPage()
    {
        this.InitializeComponent();
        ViewModel = new SessionDetailViewModel();
        this.DataContext = this;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string sessionId)
        {
            await ViewModel.InitializeAsync(sessionId);
        }
    }
}
