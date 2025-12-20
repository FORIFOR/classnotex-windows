using Microsoft.UI.Xaml.Controls;
using ClassnoteX.WinUI.ViewModels;

namespace ClassnoteX.WinUI.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        this.InitializeComponent();
        ViewModel = new DashboardViewModel();
    }

    private void RecentSessions_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ClassnoteX.Core.Models.Session session)
        {
            ViewModel.OpenSessionCommand.Execute(session);
        }
    }
}
