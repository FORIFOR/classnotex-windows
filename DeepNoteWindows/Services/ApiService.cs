using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using DeepNoteWindows.Models;

namespace DeepNoteWindows.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _authService = authService;
        _baseUrl = LoadBaseUrl();
    }

    private string LoadBaseUrl()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings?.Api.BaseUrl ?? "https://classnote-api-900324644592.asia-northeast1.run.app";
            }
        }
        catch { }
        return "https://classnote-api-900324644592.asia-northeast1.run.app";
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string endpoint)
    {
        var token = await _authService.GetValidIdTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("認証が必要です");

        var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    // ---------- Session CRUD ----------

    public async Task<Session> CreateSessionAsync(string title, string? clientSessionId = null)
    {
        var request = await CreateRequestAsync(HttpMethod.Post, "/sessions");
        request.Content = JsonContent.Create(new CreateSessionRequest
        {
            Title = title,
            ClientSessionId = clientSessionId ?? Guid.NewGuid().ToString()
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<Session>(JsonOptions);
        return session ?? throw new InvalidOperationException("セッションの作成に失敗しました");
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        var request = await CreateRequestAsync(HttpMethod.Get, $"/sessions/{sessionId}");
        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Session>(JsonOptions);
    }

    public async Task<List<Session>> GetSessionsAsync()
    {
        var request = await CreateRequestAsync(HttpMethod.Get, "/sessions");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Backend returns List[SessionResponse] (JSON array directly)
        var result = await response.Content.ReadFromJsonAsync<List<Session>>(JsonOptions);
        return result ?? new List<Session>();
    }

    // ---------- Transcript Chunks ----------

    public async Task<TranscriptChunksResponse> GetTranscriptChunksAsync(
        string sessionId,
        int? fromMs = null,
        int? toMs = null,
        string? after = null,
        int limit = 50)
    {
        var queryParams = new List<string> { $"limit={limit}" };

        if (fromMs.HasValue)
            queryParams.Add($"fromMs={fromMs.Value}");
        if (toMs.HasValue)
            queryParams.Add($"toMs={toMs.Value}");
        if (!string.IsNullOrEmpty(after))
            queryParams.Add($"after={HttpUtility.UrlEncode(after)}");

        var queryString = string.Join("&", queryParams);
        var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"/sessions/{sessionId}/transcript_chunks?{queryString}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TranscriptChunksResponse>(JsonOptions);
        return result ?? new TranscriptChunksResponse();
    }

    public async Task<AppendChunksResponse> AppendChunksAsync(string sessionId, List<ChunkInput> chunks)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Post,
            $"/sessions/{sessionId}/transcript_chunks:append");

        request.Content = JsonContent.Create(new AppendChunksRequest
        {
            Chunks = chunks,
            Source = "device",
            UpdateSessionTranscript = false,
            Finalize = false
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AppendChunksResponse>(JsonOptions);
        return result ?? new AppendChunksResponse();
    }

    // ---------- Session Lifecycle ----------

    public async Task<FinalizeResponse> FinalizeAsync(
        string sessionId,
        bool generateSummary = true,
        bool generatePlaylist = true,
        bool generateQuiz = false)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Post,
            $"/sessions/{sessionId}/finalize");

        request.Content = JsonContent.Create(new FinalizeRequest
        {
            GenerateSummary = generateSummary,
            GeneratePlaylist = generatePlaylist,
            GenerateQuiz = generateQuiz
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FinalizeResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Finalize処理に失敗しました");
    }

    // ---------- Jobs ----------

    public async Task<JobResponse> CreateJobAsync(string sessionId, string type, string? idempotencyKey = null)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Post,
            $"/sessions/{sessionId}/jobs");

        request.Content = JsonContent.Create(new JobRequest
        {
            Type = type,
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString()
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JobResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("ジョブの作成に失敗しました");
    }

    public async Task<JobResponse> GetJobStatusAsync(string sessionId, string jobIdOrType)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"/sessions/{sessionId}/jobs/{jobIdOrType}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JobResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("ジョブステータスの取得に失敗しました");
    }

    // ---------- Artifacts ----------

    public async Task<DerivedStatusResponse> GetSummaryArtifactAsync(string sessionId)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"/sessions/{sessionId}/artifacts/summary");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DerivedStatusResponse>(JsonOptions);
        return result ?? new DerivedStatusResponse();
    }

    public async Task<DerivedStatusResponse> GetQuizArtifactAsync(string sessionId)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"/sessions/{sessionId}/artifacts/quiz");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DerivedStatusResponse>(JsonOptions);
        return result ?? new DerivedStatusResponse();
    }

    public async Task<PlaylistArtifactResponse> GetPlaylistArtifactAsync(string sessionId)
    {
        var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"/sessions/{sessionId}/artifacts/playlist");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PlaylistArtifactResponse>(JsonOptions);
        return result ?? new PlaylistArtifactResponse();
    }
}
