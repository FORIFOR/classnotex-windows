using System.Text.Json.Serialization;

namespace DeepNoteWindows.Models;

/// <summary>
/// Request body for POST /sessions/{id}/jobs
/// </summary>
public class JobRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, object>? Params { get; set; }

    [JsonPropertyName("idempotencyKey")]
    public string? IdempotencyKey { get; set; }

    [JsonPropertyName("force")]
    public bool Force { get; set; } = false;
}

/// <summary>
/// Response from POST/GET /sessions/{id}/jobs/{jobId}
/// </summary>
public class JobResponse
{
    [JsonPropertyName("jobId")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("errorReason")]
    public string? ErrorReason { get; set; }

    [JsonPropertyName("result")]
    public Dictionary<string, object>? Result { get; set; }

    [JsonPropertyName("progress")]
    public double? Progress { get; set; }

    [JsonPropertyName("pollUrl")]
    public string? PollUrl { get; set; }
}

/// <summary>
/// Response from GET /sessions/{id}/artifacts/summary or /artifacts/quiz
/// </summary>
public class DerivedStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public DerivedResult? Result { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("errorReason")]
    public string? ErrorReason { get; set; }

    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }
}

public class DerivedResult
{
    [JsonPropertyName("markdown")]
    public string? Markdown { get; set; }

    [JsonPropertyName("json")]
    public object? Json { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("topicSummary")]
    public string? TopicSummary { get; set; }
}

/// <summary>
/// Response from GET /sessions/{id}/artifacts/playlist
/// </summary>
public class PlaylistArtifactResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("items")]
    public List<PlaylistItem>? Items { get; set; }

    [JsonPropertyName("errorReason")]
    public string? ErrorReason { get; set; }

    [JsonPropertyName("durationSec")]
    public double? DurationSec { get; set; }

    [JsonPropertyName("timebase")]
    public string Timebase { get; set; } = "audio_seconds";
}

/// <summary>
/// Navigation parameter to pass session + finalize info to TranscriptPage
/// </summary>
public class TranscriptPageParams
{
    public Session Session { get; set; } = null!;
    public FinalizeResponse? FinalizeResult { get; set; }
}
