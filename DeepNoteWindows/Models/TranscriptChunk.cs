using System.Text.Json.Serialization;

namespace DeepNoteWindows.Models;

public class TranscriptChunk
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("startMs")]
    public double StartMs { get; set; }

    [JsonPropertyName("endMs")]
    public double EndMs { get; set; }

    [JsonPropertyName("speakerId")]
    public string? SpeakerId { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "final";

    [JsonPropertyName("version")]
    public int? Version { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }
}

public class ChunkInput
{
    [JsonPropertyName("startMs")]
    public double StartMs { get; set; }

    [JsonPropertyName("endMs")]
    public double EndMs { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("speakerId")]
    public string? SpeakerId { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "final";
}

public class AppendChunksRequest
{
    [JsonPropertyName("chunks")]
    public List<ChunkInput> Chunks { get; set; } = new();

    [JsonPropertyName("source")]
    public string Source { get; set; } = "device";

    [JsonPropertyName("updateSessionTranscript")]
    public bool UpdateSessionTranscript { get; set; } = false;

    [JsonPropertyName("finalize")]
    public bool Finalize { get; set; } = false;
}

public class AppendChunksResponse
{
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }

    [JsonPropertyName("chunkIds")]
    public List<string> ChunkIds { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("transcriptVersion")]
    public int TranscriptVersion { get; set; }
}
