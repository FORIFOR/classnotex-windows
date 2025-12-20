using System.Threading.Tasks;

namespace ClassnoteX.Core.Interfaces;

public interface ISyncService
{
    Task SyncAsync();
    bool IsSyncing { get; }
}
