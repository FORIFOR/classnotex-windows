using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.Core.Models;
using ClassnoteX.WinUI.Services;
using System.Threading.Tasks;

namespace ClassnoteX.WinUI.ViewModels;

public partial class SessionDetailViewModel : ObservableObject
{
    private readonly ISessionRepository _sessionRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Session? selectedSession;

    [ObservableProperty]
    private bool isLoading;

    public SessionDetailViewModel()
    {
         _sessionRepository = ServiceLocator.GetService<ISessionRepository>();
         _navigationService = ServiceLocator.GetService<INavigationService>();
    }

    public async Task InitializeAsync(string sessionId)
    {
        IsLoading = true;
        await _sessionRepository.InitializeAsync();
        SelectedSession = await _sessionRepository.GetByIdAsync(sessionId);
        IsLoading = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
}
