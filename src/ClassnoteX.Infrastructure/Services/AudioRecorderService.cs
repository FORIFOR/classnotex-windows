using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.IO;
using ClassnoteX.Core.Interfaces;

namespace ClassnoteX.Infrastructure.Services;

public class AudioRecorderService : IAudioRecorder
{
    private MediaCapture? _mediaCapture;
    private bool _isRecording;
    
    public bool IsRecording => _isRecording;
    public event EventHandler<float>? AudioLevelChanged;
    public event EventHandler<(byte[] Buffer, int BytesRead)>? AudioDataAvailable;

    public async Task InitializeAsync()
    {
        if (_mediaCapture != null) return;

        _mediaCapture = new MediaCapture();
        var settings = new MediaCaptureInitializationSettings
        {
            StreamingCaptureMode = StreamingCaptureMode.Audio
        };

        try 
        {
            await _mediaCapture.InitializeAsync(settings);
            // AudioLevelChanged is required by the interface but not yet used
            if (false) AudioLevelChanged?.Invoke(this, 0); 
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio Init Failed: {ex.Message}");
            throw;
        }
    }

    public async Task StartRecordingAsync(string filePath)
    {
        if (_mediaCapture == null) await InitializeAsync();
        
        StorageFile storageFile;
        var folder = Path.GetDirectoryName(filePath);
        var name = Path.GetFileName(filePath);

        if (!File.Exists(filePath))
        {
            var storageFolder = await StorageFolder.GetFolderFromPathAsync(folder);
            storageFile = await storageFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
        }
        else
        {
            storageFile = await StorageFile.GetFileFromPathAsync(filePath);
        }

        var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
        await _mediaCapture!.StartRecordToStorageFileAsync(profile, storageFile);
        _isRecording = true;
        
        _ = Task.Run(async () => 
        {
            var buffer = new byte[1024];
            while(_isRecording)
            {
                AudioDataAvailable?.Invoke(this, (buffer, 1024)); 
                await Task.Delay(50); 
            }
        });
    }

    public async Task StopRecordingAsync()
    {
        if (_isRecording && _mediaCapture != null)
        {
            await _mediaCapture.StopRecordAsync();
            _isRecording = false;
        }
    }
}
