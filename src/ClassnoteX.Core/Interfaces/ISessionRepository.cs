using System.Collections.Generic;
using System.Threading.Tasks;
using ClassnoteX.Core.Models;

namespace ClassnoteX.Core.Interfaces;

public interface ISessionRepository
{
    Task InitializeAsync();
    Task<List<Session>> GetAllAsync();
    Task<Session?> GetByIdAsync(string id);
    Task SaveAsync(Session session);
    Task DeleteAsync(string id);
}
