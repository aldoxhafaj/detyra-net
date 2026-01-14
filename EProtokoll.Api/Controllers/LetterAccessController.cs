using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/letters/{id:int}/access")]
[Authorize(Roles = "Manager,Administrator")]
public class LetterAccessController : ControllerBase
{
    private readonly AppDbContext _db;

    public LetterAccessController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var users = await _db.LetterAccesses
            .Where(x => x.LetterId == id)
            .Select(x => x.UserId)
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int id, AccessRequest request)
    {
        var exists = await _db.LetterAccesses.AnyAsync(x => x.LetterId == id && x.UserId == request.UserId);
        if (exists)
        {
            return Ok();
        }
        _db.LetterAccesses.Add(new LetterAccess
        {
            LetterId = id,
            UserId = request.UserId
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Remove(int id, int userId)
    {
        var access = await _db.LetterAccesses.SingleOrDefaultAsync(x => x.LetterId == id && x.UserId == userId);
        if (access == null)
        {
            return NotFound();
        }
        _db.LetterAccesses.Remove(access);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
