using System.Text.Json.Serialization;

namespace DeepNoteWindows.Models;

public class FinalizeRequest
{
    [JsonPropertyName("generateSummary")]
    public bool GenerateSummary { get; set; } = true;

    [JsonPropertyName("generatePlaylist")]
    public bool GeneratePlaylist { get; set; } = true;

    [JsonPropertyName("generateQuiz")]
    public bool GenerateQuiz { get; set; } = false;
}

public class FinalizeResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("hasTranscript")]
    public bool HasTranscript { get; set; }

    [JsonPropertyName("transcriptTextLen")]
    public int TranscriptTextLen { get; set; }

    [JsonPropertyName("jobs")]
    public List<JobInfo> Jobs { get; set; } = new();
}

public class JobInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
