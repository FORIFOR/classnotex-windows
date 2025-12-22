using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ClassnoteX.Core.Models;
using System.Collections.Generic;

namespace ClassnoteX.Infrastructure.Services;

public class ClassnoteApiClient
{
    private readonly HttpClient _httpClient;

    public ClassnoteApiClient()
    {
        _httpClient = new HttpClient();
        // TODO: Set BaseAddress from config
        // _httpClient.BaseAddress = new System.Uri("https://api.classnote.app");
        
        // Silence unused field warning
        _ = _httpClient;
    }

    public async Task<List<Session>> GetSessionsAsync()
    {
        // Placeholder
        return await Task.FromResult(new List<Session>());
    }

    public async Task UploadSessionAsync(Session session)
    {
        // Placeholder
        await Task.CompletedTask;
    }
}
