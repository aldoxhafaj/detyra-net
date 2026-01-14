using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Services;

public interface INotificationService
{
    Task CreateAsync(int userId, string type, string message, int? letterId = null);
    Task<List<Notification>> GetForUserAsync(int userId, bool unreadOnly);
    Task<bool> MarkReadAsync(int id, int userId);
    Task<List<AppUser>> GetUsersInRolesAsync(params string[] roles);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(int userId, string type, string message, int? letterId = null)
    {
        var exists = await _db.Notifications.AnyAsync(x =>
            x.UserId == userId &&
            x.Type == type &&
            x.LetterId == letterId &&
            !x.IsRead);
        if (exists)
        {
            return;
        }
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            LetterId = letterId
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetForUserAsync(int userId, bool unreadOnly)
    {
        var query = _db.Notifications.AsNoTracking().Where(x => x.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(x => !x.IsRead);
        }
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<bool> MarkReadAsync(int id, int userId)
    {
        var notification = await _db.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (notification == null)
        {
            return false;
        }
        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<AppUser>> GetUsersInRolesAsync(params string[] roles)
    {
        return await _db.Users.AsNoTracking().Where(x => roles.Contains(x.Role) && x.IsActive).ToListAsync();
    }
}
