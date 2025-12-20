using System;
using Microsoft.UI.Xaml.Controls;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.WinUI.Views;
using System.Collections.Generic;

namespace ClassnoteX.WinUI.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    private readonly Dictionary<string, Type> _pages = new()
    {
        { "DashboardPage", typeof(DashboardPage) },
        { "SessionListPage", typeof(SessionListPage) },
        { "RecordingPage", typeof(RecordingPage) },
        { "SessionDetailPage", typeof(SessionDetailPage) }
    };

    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo(string pageKey, object? parameter = null)
    {
        if (_frame == null) return;

        if (_pages.TryGetValue(pageKey, out var pageType))
        {
            _frame.Navigate(pageType, parameter);
        }
    }

    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }
}
