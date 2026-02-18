using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepNoteWindows.Models;
using DeepNoteWindows.Services;
using System.Collections.ObjectModel;

namespace DeepNoteWindows.ViewModels;

public partial class SessionListViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _userName = string.Empty;

    public ObservableCollection<Session> Sessions { get; } = new();

    public SessionListViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;

        if (_authService.CurrentUser != null)
        {
            UserName = _authService.CurrentUser.DisplayName ?? _authService.CurrentUser.Email ?? "ユーザー";
        }
    }

    [RelayCommand]
    public async Task LoadSessionsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var sessions = await _apiService.GetSessionsAsync();
            Sessions.Clear();

            foreach (var session in sessions.OrderByDescending(s => s.CreatedAt))
            {
                Sessions.Add(session);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"セッションの取得に失敗しました: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreateSessionAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "タイトルを入力してください";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var session = await _apiService.CreateSessionAsync(title);
            Sessions.Insert(0, session);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"セッションの作成に失敗しました: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        await _authService.SignOutAsync();
    }
}
