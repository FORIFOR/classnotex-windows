using DeepNoteWindows.Models;

namespace DeepNoteWindows.Services;

public interface IApiService
{
    // Session CRUD
    Task<Session> CreateSessionAsync(string title, string? clientSessionId = null);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<List<Session>> GetSessionsAsync();

    // Transcript chunks
    Task<TranscriptChunksResponse> GetTranscriptChunksAsync(
        string sessionId,
        int? fromMs = null,
        int? toMs = null,
        string? after = null,
        int limit = 50);
    Task<AppendChunksResponse> AppendChunksAsync(string sessionId, List<ChunkInput> chunks);

    // Session lifecycle
    Task<FinalizeResponse> FinalizeAsync(
        string sessionId,
        bool generateSummary = true,
        bool generatePlaylist = true,
        bool generateQuiz = false);

    // Jobs
    Task<JobResponse> CreateJobAsync(string sessionId, string type, string? idempotencyKey = null);
    Task<JobResponse> GetJobStatusAsync(string sessionId, string jobIdOrType);

    // Artifacts
    Task<DerivedStatusResponse> GetSummaryArtifactAsync(string sessionId);
    Task<DerivedStatusResponse> GetQuizArtifactAsync(string sessionId);
    Task<PlaylistArtifactResponse> GetPlaylistArtifactAsync(string sessionId);
}
