using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Pages;

public class NotificationsModel : PageModel
{
    private readonly AppDbContext _db;

    public NotificationsModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Notification> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _db.Notifications
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
}
