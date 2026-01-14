using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Pages;

public class LettersModel : PageModel
{
    private readonly AppDbContext _db;

    public LettersModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Letter> Letters { get; set; } = new();

    public async Task OnGetAsync()
    {
        Letters = await _db.Letters
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
}
