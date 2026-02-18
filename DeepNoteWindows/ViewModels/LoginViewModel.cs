using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepNoteWindows.Services;

namespace DeepNoteWindows.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public string GoogleAuthUrl => ((AuthService)_authService).GetGoogleAuthUrl();

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> HandleAuthCodeAsync(string authCode)
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "認証中...";

        try
        {
            var success = await ((AuthService)_authService).ExchangeCodeForTokenAsync(authCode);

            if (success)
            {
                StatusMessage = "ログイン成功！";
                return true;
            }
            else
            {
                ErrorMessage = "認証に失敗しました。もう一度お試しください。";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"エラー: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
