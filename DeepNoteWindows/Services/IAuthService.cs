using DeepNoteWindows.Models;

namespace DeepNoteWindows.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    string? IdToken { get; }
    UserInfo? CurrentUser { get; }

    event EventHandler<bool>? AuthStateChanged;

    // Google Sign-In
    string GetGoogleAuthUrl();
    Task<bool> SignInWithGoogleAsync();
    Task<bool> ExchangeCodeForTokenAsync(string authCode);

    // Apple Sign-In
    string GetAppleAuthUrl();
    Task<bool> ExchangeAppleTokenAsync(string idToken);

    // Token management
    Task<bool> RefreshTokenAsync();
    Task<string?> GetValidIdTokenAsync();
    Task SignOutAsync();
}
