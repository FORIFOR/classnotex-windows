using System.Text.Json.Serialization;

namespace DeepNoteWindows.Models;

public class PagedResponse<T>
{
    [JsonPropertyName("chunks")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

public class TranscriptChunksResponse
{
    [JsonPropertyName("chunks")]
    public List<TranscriptChunk> Chunks { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}
