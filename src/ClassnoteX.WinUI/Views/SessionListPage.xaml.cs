using Microsoft.UI.Xaml.Controls;
using ClassnoteX.WinUI.ViewModels;

namespace ClassnoteX.WinUI.Views;

public sealed partial class SessionListPage : Page
{
    public SessionListViewModel ViewModel { get; }

    public SessionListPage()
    {
        this.InitializeComponent();
        ViewModel = new SessionListViewModel();
    }
}
