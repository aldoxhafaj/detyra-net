using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/departments")]
[Authorize(Roles = "Administrator")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DepartmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Departments.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Departments.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DepartmentRequest request)
    {
        var entity = new Department { Name = request.Name };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DepartmentRequest request)
    {
        var entity = await _db.Departments.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        entity.Name = request.Name;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Departments.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        _db.Departments.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
