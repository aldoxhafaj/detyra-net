using EProtokoll.Api.Data;
using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/protocol-books")]
[Authorize(Roles = "Administrator")]
public class ProtocolBooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IProtocolService _protocol;

    public ProtocolBooksController(AppDbContext db, IProtocolService protocol)
    {
        _db = db;
        _protocol = protocol;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.ProtocolBooks.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] int year)
    {
        var book = await _protocol.OpenBookAsync(year);
        return Ok(book);
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] int year)
    {
        var closed = await _protocol.CloseBookAsync(year);
        if (!closed)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpGet("{year:int}/print")]
    public async Task<IActionResult> Print(int year)
    {
        var letters = await _db.Letters
            .Where(x => x.CreatedAt.Year == year)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var lines = new List<string> { "ProtocolNumber,Type,Classification,Subject,CreatedAt,Status" };
        lines.AddRange(letters.Select(x =>
            $"{x.ProtocolNumber},{x.Type},{x.Classification},\"{x.Subject.Replace("\"", "\"\"")}\",{x.CreatedAt:O},{x.Status}"));
        var content = string.Join(Environment.NewLine, lines);
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/csv", $"protocol-book-{year}.csv");
    }

    [HttpGet("{year:int}/items")]
    public async Task<IActionResult> Items(int year)
    {
        var letters = await _db.Letters
            .Where(x => x.CreatedAt.Year == year)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        return Ok(letters);
    }
}
