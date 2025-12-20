using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.Core.Models;
using ClassnoteX.Infrastructure.Entities;
using Windows.Storage;

namespace ClassnoteX.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private SQLiteAsyncConnection? _db;

    public async Task InitializeAsync()
    {
        if (_db != null) return;

        var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "classnotex.db");
        _db = new SQLiteAsyncConnection(dbPath);
        
        await _db.CreateTableAsync<SessionEntity>();
        // Add TranscriptSegmentEntity table later
    }

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db == null) await InitializeAsync();
        return _db!;
    }

    public async Task<List<Session>> GetAllAsync()
    {
        var db = await GetDbAsync();
        var entities = await db.Table<SessionEntity>().OrderByDescending(x => x.CreatedAtTicks).ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<Session?> GetByIdAsync(string id)
    {
        var db = await GetDbAsync();
        var entity = await db.Table<SessionEntity>().FirstOrDefaultAsync(x => x.Id == id);
        return entity?.ToDomain();
    }

    public async Task SaveAsync(Session session)
    {
        var db = await GetDbAsync();
        var entity = SessionEntity.FromDomain(session);
        await db.InsertOrReplaceAsync(entity);
    }

    public async Task DeleteAsync(string id)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<SessionEntity>(id);
    }
}
