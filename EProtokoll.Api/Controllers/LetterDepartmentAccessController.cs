using EProtokoll.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/letters/{id:int}/department-access")]
[Authorize(Roles = "Manager,Administrator")]
public class LetterDepartmentAccessController : ControllerBase
{
    private readonly AppDbContext _db;

    public LetterDepartmentAccessController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var departments = await _db.LetterDepartmentAccesses
            .Where(x => x.LetterId == id)
            .Select(x => x.DepartmentId)
            .ToListAsync();
        return Ok(departments);
    }

    [HttpPost("{departmentId:int}")]
    public async Task<IActionResult> Add(int id, int departmentId)
    {
        var exists = await _db.LetterDepartmentAccesses.AnyAsync(x => x.LetterId == id && x.DepartmentId == departmentId);
        if (exists)
        {
            return Ok();
        }
        _db.LetterDepartmentAccesses.Add(new Models.LetterDepartmentAccess
        {
            LetterId = id,
            DepartmentId = departmentId
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{departmentId:int}")]
    public async Task<IActionResult> Remove(int id, int departmentId)
    {
        var access = await _db.LetterDepartmentAccesses.SingleOrDefaultAsync(x => x.LetterId == id && x.DepartmentId == departmentId);
        if (access == null)
        {
            return NotFound();
        }
        _db.LetterDepartmentAccesses.Remove(access);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
