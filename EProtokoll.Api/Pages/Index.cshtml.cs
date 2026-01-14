using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public int TotalLetters { get; set; }
    public int Incoming { get; set; }
    public int Outgoing { get; set; }
    public int Internal { get; set; }
    public List<Letter> LatestLetters { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalLetters = await _db.Letters.CountAsync();
        Incoming = await _db.Letters.CountAsync(x => x.Type == LetterType.Incoming);
        Outgoing = await _db.Letters.CountAsync(x => x.Type == LetterType.Outgoing);
        Internal = await _db.Letters.CountAsync(x => x.Type == LetterType.Internal);
        LatestLetters = await _db.Letters
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
}
