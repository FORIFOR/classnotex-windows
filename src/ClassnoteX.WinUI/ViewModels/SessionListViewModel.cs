using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ClassnoteX.Core.Models;
using System.Collections.Generic;

namespace ClassnoteX.WinUI.ViewModels;

public partial class SessionListViewModel : ObservableObject
{
    public ObservableCollection<Session> Sessions { get; } = new();

    [ObservableProperty]
    private string searchQuery = "";

    public SessionListViewModel()
    {
        // Mock Data
        Sessions.Add(new Session { Title = "Project Kickoff", Mode = SessionMode.Meeting, Status = SessionStatus.Completed });
        Sessions.Add(new Session { Title = "Physics 101", Mode = SessionMode.Lecture, Status = SessionStatus.Completed });
        Sessions.Add(new Session { Title = "Team Retro", Mode = SessionMode.Meeting, Status = SessionStatus.Completed });
    }

    [RelayCommand]
    private void Search()
    {
        // Todo: Filter logic
    }
}
