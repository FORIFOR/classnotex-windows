using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using DeepNoteWindows.Models;
using Windows.Security.Credentials;

namespace DeepNoteWindows.Services;

public enum AuthProvider
{
    Google,
    Apple
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;

    private string? _idToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry;
    private UserInfo? _currentUser;
    private string? _currentNonce;

    private const string CredentialResource = "DeepNoteWindows";
    private const string RefreshTokenKey = "RefreshToken";

    public bool IsAuthenticated => !string.IsNullOrEmpty(_idToken) && _tokenExpiry > DateTime.UtcNow;
    public string? IdToken => _idToken;
    public UserInfo? CurrentUser => _currentUser;

    public event EventHandler<bool>? AuthStateChanged;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _settings = LoadSettings();

        // 保存されたリフレッシュトークンを読み込み
        LoadSavedToken();
    }

    private AppSettings LoadSettings()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    private void LoadSavedToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(CredentialResource, RefreshTokenKey);
            credential.RetrievePassword();
            _refreshToken = credential.Password;
        }
        catch
        {
            _refreshToken = null;
        }
    }

    private void SaveRefreshToken(string token)
    {
        try
        {
            var vault = new PasswordVault();

            // 既存の認証情報を削除
            try
            {
                var existing = vault.Retrieve(CredentialResource, RefreshTokenKey);
                vault.Remove(existing);
            }
            catch { }

            vault.Add(new PasswordCredential(CredentialResource, RefreshTokenKey, token));
        }
        catch { }
    }

    private void ClearSavedToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(CredentialResource, RefreshTokenKey);
            vault.Remove(credential);
        }
        catch { }
    }

    // MARK: - Google Sign-In

    public string GetGoogleAuthUrl()
    {
        var redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        var scope = "openid email profile";

        var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                  $"?client_id={HttpUtility.UrlEncode(_settings.Google.ClientId)}" +
                  $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                  $"&response_type=code" +
                  $"&scope={HttpUtility.UrlEncode(scope)}" +
                  $"&access_type=offline" +
                  $"&prompt=consent";

        return url;
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        // このメソッドはWebView2からのコールバックで呼ばれる
        // 実際の認証フローはLoginPageで処理
        return IsAuthenticated;
    }

    public async Task<bool> ExchangeCodeForTokenAsync(string authCode)
    {
        try
        {
            var redirectUri = "urn:ietf:wg:oauth:2.0:oob";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = authCode,
                ["client_id"] = _settings.Google.ClientId,
                ["client_secret"] = _settings.Google.ClientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            });

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token", content);

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            if (tokenResponse == null)
                return false;

            // GoogleのID TokenでFirebaseにサインイン
            return await SignInToFirebaseWithIdToken(tokenResponse.IdToken, "google.com");
        }
        catch
        {
            return false;
        }
    }

    // MARK: - Apple Sign-In

    public string GetAppleAuthUrl()
    {
        // Generate nonce for security
        _currentNonce = GenerateNonce();
        var hashedNonce = Sha256Hash(_currentNonce);

        var redirectUri = "https://classnote-api-900324644592.asia-northeast1.run.app/auth/apple/callback";
        var scope = "name email";
        var clientId = "jp.horioshuhei.deepnote"; // Service ID for Sign in with Apple

        var url = $"https://appleid.apple.com/auth/authorize" +
                  $"?client_id={HttpUtility.UrlEncode(clientId)}" +
                  $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                  $"&response_type=code id_token" +
                  $"&response_mode=form_post" +
                  $"&scope={HttpUtility.UrlEncode(scope)}" +
                  $"&nonce={hashedNonce}";

        return url;
    }

    public async Task<bool> ExchangeAppleTokenAsync(string idToken)
    {
        return await SignInToFirebaseWithIdToken(idToken, "apple.com", _currentNonce);
    }

    private string GenerateNonce(int length = 32)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-._";
        var random = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        var sb = new StringBuilder(length);
        foreach (var b in random)
        {
            sb.Append(chars[b % chars.Length]);
        }
        return sb.ToString();
    }

    private string Sha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    // MARK: - Firebase Authentication

    private async Task<bool> SignInToFirebaseWithIdToken(string idToken, string providerId, string? nonce = null)
    {
        try
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={_settings.Firebase.ApiKey}";

            var postBody = $"id_token={idToken}&providerId={providerId}";
            if (!string.IsNullOrEmpty(nonce))
            {
                postBody += $"&nonce={nonce}";
            }

            var request = new
            {
                postBody,
                requestUri = "http://localhost",
                returnIdpCredential = true,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Firebase auth error: {error}");
                return false;
            }

            var firebaseResponse = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();
            if (firebaseResponse == null)
                return false;

            _idToken = firebaseResponse.IdToken;
            _refreshToken = firebaseResponse.RefreshToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(firebaseResponse.ExpiresIn) - 60);

            _currentUser = new UserInfo
            {
                UserId = firebaseResponse.LocalId,
                Email = firebaseResponse.Email,
                DisplayName = firebaseResponse.DisplayName,
                PhotoUrl = firebaseResponse.PhotoUrl
            };

            SaveRefreshToken(_refreshToken);
            AuthStateChanged?.Invoke(this, true);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Firebase auth exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return false;

        try
        {
            var url = $"https://securetoken.googleapis.com/v1/token?key={_settings.Firebase.ApiKey}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = _refreshToken
            });

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                await SignOutAsync();
                return false;
            }

            var refreshResponse = await response.Content.ReadFromJsonAsync<FirebaseRefreshResponse>();
            if (refreshResponse == null)
                return false;

            _idToken = refreshResponse.IdToken;
            _refreshToken = refreshResponse.RefreshToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(refreshResponse.ExpiresIn) - 60);

            SaveRefreshToken(_refreshToken);
            AuthStateChanged?.Invoke(this, true);

            return true;
        }
        catch
        {
            await SignOutAsync();
            return false;
        }
    }

    public async Task<string?> GetValidIdTokenAsync()
    {
        if (_tokenExpiry <= DateTime.UtcNow.AddMinutes(5))
        {
            if (!await RefreshTokenAsync())
                return null;
        }
        return _idToken;
    }

    public Task SignOutAsync()
    {
        _idToken = null;
        _refreshToken = null;
        _currentUser = null;
        _tokenExpiry = DateTime.MinValue;
        _currentNonce = null;

        ClearSavedToken();
        AuthStateChanged?.Invoke(this, false);

        return Task.CompletedTask;
    }
}

public class AppSettings
{
    public FirebaseSettings Firebase { get; set; } = new();
    public GoogleSettings Google { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public SherpaOnnxSettings SherpaOnnx { get; set; } = new();
}

public class FirebaseSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string AuthDomain { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = "https://classnote-api-900324644592.asia-northeast1.run.app";
}

public class SherpaOnnxSettings
{
    public string ModelPath { get; set; } = "models/sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01";
}
