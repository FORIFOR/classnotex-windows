namespace DeepNoteWindows.Services;

public interface ISherpaOnnxService
{
    bool IsModelLoaded { get; }
    Task<bool> LoadModelAsync(string modelPath);
    Task<string> TranscribeAsync(float[] samples, int sampleRate);
    void UnloadModel();
}
