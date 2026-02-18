using Microsoft.UI.Xaml.Controls;

namespace DeepNoteWindows.Helpers;

public static class NavigationHelper
{
    public static Frame? RootFrame { get; set; }

    public static void Navigate(Type pageType, object? parameter = null)
    {
        RootFrame?.Navigate(pageType, parameter);
    }

    public static void GoBack()
    {
        if (RootFrame?.CanGoBack == true)
        {
            RootFrame.GoBack();
        }
    }

    public static bool CanGoBack => RootFrame?.CanGoBack == true;
}
