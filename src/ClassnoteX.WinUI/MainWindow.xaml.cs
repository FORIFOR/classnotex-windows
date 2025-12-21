using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using ClassnoteX.WinUI.Views;
using ClassnoteX.WinUI.Services;
using ClassnoteX.Core.Interfaces;

namespace ClassnoteX.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "ClassnoteX";
        
        var navService = ServiceLocator.GetService<INavigationService>() as NavigationService;
        navService?.Initialize(ContentFrame);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Navigate to Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];
        NavView_Navigate("DashboardPage", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavView_Navigate("SettingsPage", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer != null)
        {
            var navItemTag = args.SelectedItemContainer.Tag?.ToString() ?? string.Empty;
            NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_Navigate(string navItemTag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
    {
        Type? _page = null;
        if (navItemTag == "DashboardPage")
        {
             _page = typeof(DashboardPage);
        }
        else if (navItemTag == "SessionListPage")
        {
             _page = typeof(SessionListPage);
        }
        else if (navItemTag == "CalendarPage")
        {
             // _page = typeof(CalendarPage); // TODO
             _page = typeof(DashboardPage); // Placeholder
        }
        else if (navItemTag == "SettingsPage")
        {
             // _page = typeof(SettingsPage); // TODO
             _page = typeof(DashboardPage); // Placeholder
        }

        if (_page != null)
        {
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }
    }
}
