using System;

namespace ClassnoteX.Core.Interfaces;

public interface INavigationService
{
    void NavigateTo(string pageKey, object? parameter = null);
    void GoBack();
}
