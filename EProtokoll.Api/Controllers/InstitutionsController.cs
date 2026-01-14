using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/institutions")]
[Authorize]
public class InstitutionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public InstitutionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.ExternalInstitutions.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.ExternalInstitutions.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(InstitutionRequest request)
    {
        var entity = new ExternalInstitution
        {
            Name = request.Name,
            ExternalId = request.ExternalId,
            Address = request.Address,
            Contact = request.Contact
        };
        _db.ExternalInstitutions.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, InstitutionRequest request)
    {
        var entity = await _db.ExternalInstitutions.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        entity.Name = request.Name;
        entity.ExternalId = request.ExternalId;
        entity.Address = request.Address;
        entity.Contact = request.Contact;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.ExternalInstitutions.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        _db.ExternalInstitutions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
