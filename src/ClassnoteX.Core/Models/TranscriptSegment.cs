using System;

namespace ClassnoteX.Core.Models;

public class TranscriptSegment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    
    public double StartSeconds { get; set; }
    public double EndSeconds { get; set; }
    
    public string Text { get; set; } = string.Empty;
    public string SpeakerId { get; set; } = "0";
    public float Confidence { get; set; }
    
    /// <summary>
    /// True if this segment is final, false if it's a draft update
    /// </summary>
    public bool IsFinal { get; set; }
}
