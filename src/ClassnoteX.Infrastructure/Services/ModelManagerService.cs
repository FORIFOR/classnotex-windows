using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace ClassnoteX.Infrastructure.Services;

public class ModelManagerService
{
    private const string ManifestPath = "models/manifest.json";
    
    // Todo: Get from AppData
    private string GetModelDirectory() => Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "models");

    public async Task<bool> AreModelsPresentAsync()
    {
        var dir = GetModelDirectory();
        if (!Directory.Exists(dir)) return false;

        // Check for key files (simplified)
        return File.Exists(Path.Combine(dir, "encoder.onnx")) && 
               File.Exists(Path.Combine(dir, "decoder.onnx"));
    }

    public async Task DownloadModelsAsync(IProgress<double> progress)
    {
        var dir = GetModelDirectory();
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // Read Manifest (Embedded or from file)
        // For now, assuming manifest is deployed with app
        var manifestText = await File.ReadAllTextAsync(ManifestPath); 
        // Note: In packaged app, ManifestPath might need `Package.Current.InstalledLocation`
        
        using var client = new HttpClient();
        
        // Mock download loop using manifest logic
        // var manifest = JsonSerializer.Deserialize<Manifest>(manifestText);
        
        // Mock Progress
        for(int i=0; i<=100; i+=10)
        {
            progress.Report(i);
            await Task.Delay(200);
        }
    }
}
