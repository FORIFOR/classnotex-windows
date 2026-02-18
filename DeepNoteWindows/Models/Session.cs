using System.Text.Json.Serialization;

namespace DeepNoteWindows.Models;

public class Session
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("clientSessionId")]
    public string? ClientSessionId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "lecture";

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("endedAt")]
    public DateTime? EndedAt { get; set; }

    [JsonPropertyName("durationSec")]
    public double? DurationSec { get; set; }

    // Transcript
    [JsonPropertyName("hasTranscript")]
    public bool HasTranscript { get; set; }

    [JsonPropertyName("transcriptText")]
    public string? TranscriptText { get; set; }

    [JsonPropertyName("transcriptTextLen")]
    public int TranscriptTextLen { get; set; }

    [JsonPropertyName("transcriptChunkCount")]
    public int TranscriptChunkCount { get; set; }

    // Audio
    [JsonPropertyName("hasAudio")]
    public bool HasAudio { get; set; }

    // Ownership
    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("isOwner")]
    public bool? IsOwner { get; set; }

    [JsonPropertyName("canManage")]
    public bool? CanManage { get; set; }

    // Job statuses
    [JsonPropertyName("summaryStatus")]
    public string? SummaryStatus { get; set; }

    [JsonPropertyName("quizStatus")]
    public string? QuizStatus { get; set; }

    [JsonPropertyName("playlistStatus")]
    public string? PlaylistStatus { get; set; }

    // AI Results (available in detail response)
    [JsonPropertyName("summaryMarkdown")]
    public string? SummaryMarkdown { get; set; }

    [JsonPropertyName("summaryError")]
    public string? SummaryError { get; set; }

    [JsonPropertyName("quizMarkdown")]
    public string? QuizMarkdown { get; set; }

    [JsonPropertyName("quizError")]
    public string? QuizError { get; set; }

    [JsonPropertyName("playlist")]
    public List<PlaylistItem>? Playlist { get; set; }

    // Tags
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    // Flags
    [JsonPropertyName("hasSummary")]
    public bool HasSummary { get; set; }

    [JsonPropertyName("hasQuiz")]
    public bool HasQuiz { get; set; }

    // Legacy field (backend returns summaryMarkdown, not "summary")
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}

public class CreateSessionRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("clientSessionId")]
    public string? ClientSessionId { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "lecture";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "windows";
}

public class PlaylistItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("startSec")]
    public double StartSec { get; set; }

    [JsonPropertyName("endSec")]
    public double EndSec { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
