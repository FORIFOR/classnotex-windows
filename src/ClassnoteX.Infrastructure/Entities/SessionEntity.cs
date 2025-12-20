using SQLite;
using System;
using ClassnoteX.Core.Models;

namespace ClassnoteX.Infrastructure.Entities;

[Table("Sessions")]
public class SessionEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Mode { get; set; } // Map to Enum
    public long CreatedAtTicks { get; set; }
    public double DurationSeconds { get; set; }
    public int Status { get; set; } // Map to Enum
    
    public string? LocalAudioPath { get; set; }
    public string? CloudAudioUrl { get; set; }
    public string? Summary { get; set; }
    
    // Simple tag storage (comma separated)
    public string? TagsJson { get; set; }
    
    public bool IsClientDirty { get; set; }
    public long? LastSyncedAtTicks { get; set; }

    public Session ToDomain()
    {
        return new Session
        {
            Id = Id,
            Title = Title,
            Mode = (SessionMode)Mode,
            CreatedAt = new DateTime(CreatedAtTicks),
            DurationSeconds = DurationSeconds,
            Status = (SessionStatus)Status,
            LocalAudioPath = LocalAudioPath,
            CloudAudioUrl = CloudAudioUrl,
            Summary = Summary,
            IsClientDirty = IsClientDirty,
            LastSyncedAt = LastSyncedAtTicks.HasValue ? new DateTime(LastSyncedAtTicks.Value) : null
            // Todo: Tags
        };
    }

    public static SessionEntity FromDomain(Session session)
    {
        return new SessionEntity
        {
            Id = session.Id,
            Title = session.Title,
            Mode = (int)session.Mode,
            CreatedAtTicks = session.CreatedAt.Ticks,
            DurationSeconds = session.DurationSeconds,
            Status = (int)session.Status,
            LocalAudioPath = session.LocalAudioPath,
            CloudAudioUrl = session.CloudAudioUrl,
            Summary = session.Summary,
            IsClientDirty = session.IsClientDirty,
            LastSyncedAtTicks = session.LastSyncedAt?.Ticks
        };
    }
}
