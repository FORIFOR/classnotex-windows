using System;

namespace ClassnoteX.Core.Models;

public enum SessionStatus
{
    Recording,
    Processing,
    Completed,
    Error
}

public enum SessionMode
{
    Meeting,
    Lecture
}

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Session";
    public SessionMode Mode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double DurationSeconds { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Recording;
    
    // File Paths
    public string? LocalAudioPath { get; set; }
    public string? CloudAudioUrl { get; set; }
    
    // AI Data
    public string? Summary { get; set; }
    public string[]? Tags { get; set; }
    
    // Sync
    public bool IsClientDirty { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
