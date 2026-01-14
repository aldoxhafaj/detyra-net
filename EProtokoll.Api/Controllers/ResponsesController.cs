using System.Security.Claims;
using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/letters/{id:int}/responses")]
[Authorize]
public class ResponsesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResponsesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int id)
    {
        var letter = await _db.Letters.FindAsync(id);
        if (letter == null)
        {
            return NotFound();
        }
        if (letter.Classification == DocumentClassification.Secret && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }
        if (letter.Classification == DocumentClassification.Restricted && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Forbid();
            }
            var departmentId = await GetUserDepartmentIdAsync(userId.Value);
            var allowed = letter.CreatedByUserId == userId || letter.AssignedToUserId == userId ||
                          await _db.LetterAccesses.AnyAsync(x => x.LetterId == letter.Id && x.UserId == userId) ||
                          (departmentId != null &&
                           await _db.LetterDepartmentAccesses.AnyAsync(x => x.LetterId == letter.Id && x.DepartmentId == departmentId));
            if (!allowed)
            {
                return Forbid();
            }
        }
        var responses = await _db.Responses.Where(x => x.LetterId == id).ToListAsync();
        return Ok(responses);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int id, ResponseRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }
        var letter = await _db.Letters.FindAsync(id);
        if (letter == null)
        {
            return NotFound();
        }
        if (letter.Classification == DocumentClassification.Secret && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }
        if (letter.Classification == DocumentClassification.Restricted && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            var departmentId = await GetUserDepartmentIdAsync(userId.Value);
            var allowed = letter.CreatedByUserId == userId || letter.AssignedToUserId == userId ||
                          await _db.LetterAccesses.AnyAsync(x => x.LetterId == letter.Id && x.UserId == userId) ||
                          (departmentId != null &&
                           await _db.LetterDepartmentAccesses.AnyAsync(x => x.LetterId == letter.Id && x.DepartmentId == departmentId));
            if (!allowed)
            {
                return Forbid();
            }
        }
        var entry = new ResponseEntry
        {
            LetterId = id,
            UserId = userId.Value,
            Message = request.Message
        };
        _db.Responses.Add(entry);
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = id,
            Action = "Response",
            UserId = userId.Value,
            Note = request.Message
        });
        await _db.SaveChangesAsync();
        return Ok(entry);
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
        if (int.TryParse(id, out var userId))
        {
            return userId;
        }
        return null;
    }

    private async Task<int?> GetUserDepartmentIdAsync(int userId)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);
        return user?.DepartmentId;
    }
}
