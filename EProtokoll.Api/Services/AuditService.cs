using EProtokoll.Api.Data;
using EProtokoll.Api.Models;

namespace EProtokoll.Api.Services;

public interface IAuditService
{
    Task LogAsync(string entityName, int entityId, string action, int userId, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string entityName, int entityId, string action, int userId, string? details = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            Details = details
        });
        await _db.SaveChangesAsync();
    }
}
