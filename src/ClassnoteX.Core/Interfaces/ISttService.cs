using System;
using System.Threading.Tasks;
using ClassnoteX.Core.Models;

namespace ClassnoteX.Core.Interfaces;

public interface ISttService
{
    Task InitializeAsync();
    
    /// <summary>
    /// Processes audio buffer and returns segments.
    /// </summary>
    Task ProcessAudioBufferAsync(byte[] buffer, int bytesRead);
    
    event EventHandler<TranscriptSegment> SegmentReceived;
}
