using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ClassnoteX.Core.Models;
using System;
using ClassnoteX.WinUI.Services;
using ClassnoteX.Core.Interfaces;

namespace ClassnoteX.WinUI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string welcomeMessage = "Welcome back, Saiteku";

    public ObservableCollection<Session> RecentSessions { get; } = new();

    private readonly INavigationService _navigationService;

    private readonly INavigationService _navigationService;
    private readonly ISessionRepository _sessionRepository;

    public DashboardViewModel()
    {
        _navigationService = ServiceLocator.GetService<INavigationService>();
        _sessionRepository = ServiceLocator.GetService<ISessionRepository>();
        
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        await _sessionRepository.InitializeAsync();
        var sessions = await _sessionRepository.GetAllAsync();
        
        // Use dispatcher if needed, but ObservableCollection usually needs basic thread awareness
        // For MVP, just clear and add
        RecentSessions.Clear();
        foreach(var s in sessions) RecentSessions.Add(s);
    }

    [RelayCommand]
    private void StartRecording(string mode)
    {
        _navigationService.NavigateTo("RecordingPage", mode);
    }

    [RelayCommand]
    private void OpenSession(Session session)
    {
        if (session != null)
        {
            _navigationService.NavigateTo("SessionDetailPage", session.Id);
        }
    }
}
