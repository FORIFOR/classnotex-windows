using System.Threading.Tasks;
using ClassnoteX.Core.Interfaces;

namespace ClassnoteX.Infrastructure.Services;

public class SyncService : ISyncService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ClassnoteApiClient _apiClient;
    
    public bool IsSyncing { get; private set; }

    public SyncService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
        _apiClient = new ClassnoteApiClient();
        
        // Silence unused field warnings for now as this is a placeholder
        _ = _sessionRepository;
        _ = _apiClient;
    }

    public async Task SyncAsync()
    {
        if (IsSyncing) return;
        IsSyncing = true;

        try 
        {
            // 1. Get dirty local sessions
            // 2. Upload to Cloud
            // 3. Get changes from Cloud
            // 4. Update local DB
            await Task.Delay(100); // Placeholder
        }
        finally
        {
            IsSyncing = false;
        }
    }
}
