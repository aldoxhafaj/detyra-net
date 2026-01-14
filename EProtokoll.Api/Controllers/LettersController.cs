using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/letters")]
[Authorize]
public class LettersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IProtocolService _protocol;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;

    public LettersController(AppDbContext db, IProtocolService protocol, IAuditService audit, INotificationService notifications)
    {
        _db = db;
        _protocol = protocol;
        _audit = audit;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] LetterType? type,
        [FromQuery] LetterStatus? status,
        [FromQuery] DocumentClassification? classification,
        [FromQuery] int? assignedTo,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _db.Letters.AsNoTracking().AsQueryable();
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (classification.HasValue) query = query.Where(x => x.Classification == classification);
        if (assignedTo.HasValue) query = query.Where(x => x.AssignedToUserId == assignedTo);
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value);

        if (!User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Ok(new List<Letter>());
            }
            var departmentId = await GetUserDepartmentIdAsync(userId.Value);
            query = query.Where(x => x.Classification != DocumentClassification.Secret);
            query = query.Where(x =>
                x.Classification != DocumentClassification.Restricted ||
                x.CreatedByUserId == userId ||
                x.AssignedToUserId == userId ||
                _db.LetterAccesses.Any(a => a.LetterId == x.Id && a.UserId == userId) ||
                (departmentId != null &&
                 _db.LetterDepartmentAccesses.Any(a => a.LetterId == x.Id && a.DepartmentId == departmentId)));
        }

        var items = await query.ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var letter = await _db.Letters.Include(x => x.Documents).FirstOrDefaultAsync(x => x.Id == id);
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
        return Ok(letter);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLetterRequest request)
    {
        if (request.Classification == DocumentClassification.Secret && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        var userId = GetUserId() ?? request.CreatedByUserId;
        var year = DateTime.UtcNow.Year;
        var protocolNumber = await _protocol.NextProtocolNumberAsync(year);
        var letter = new Letter
        {
            Type = request.Type,
            Classification = request.Classification,
            Subject = request.Subject,
            ExternalInstitutionId = request.ExternalInstitutionId,
            CreatedByUserId = userId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            OutgoingChannel = request.OutgoingChannel,
            OutgoingDate = request.OutgoingDate,
            OutgoingReference = request.OutgoingReference,
            ProtocolNumber = protocolNumber
        };
        _db.Letters.Add(letter);
        await _db.SaveChangesAsync();
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = letter.Id,
            Action = "Created",
            UserId = userId
        });
        if (letter.Classification == DocumentClassification.Restricted)
        {
            _db.LetterAccesses.Add(new LetterAccess { LetterId = letter.Id, UserId = userId });
            var departmentId = await GetUserDepartmentIdAsync(userId);
            if (departmentId != null)
            {
                _db.LetterDepartmentAccesses.Add(new LetterDepartmentAccess { LetterId = letter.Id, DepartmentId = departmentId.Value });
            }
        }
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Letter", letter.Id, "Created", userId, protocolNumber);
        var managers = await _notifications.GetUsersInRolesAsync("Manager", "Administrator");
        foreach (var manager in managers)
        {
            await _notifications.CreateAsync(manager.Id, "NewLetter", $"New letter: {letter.ProtocolNumber} - {letter.Subject}", letter.Id);
        }
        return CreatedAtAction(nameof(GetById), new { id = letter.Id }, letter);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateLetterRequest request)
    {
        var letter = await _db.Letters.FindAsync(id);
        if (letter == null)
        {
            return NotFound();
        }
        if (request.Classification == DocumentClassification.Secret && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }
        if (request.Classification == DocumentClassification.Restricted && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            var requesterId = GetUserId();
            if (!requesterId.HasValue)
            {
                return Forbid();
            }
            var departmentId = await GetUserDepartmentIdAsync(requesterId.Value);
            var allowed = letter.CreatedByUserId == requesterId || letter.AssignedToUserId == requesterId ||
                          await _db.LetterAccesses.AnyAsync(x => x.LetterId == letter.Id && x.UserId == requesterId) ||
                          (departmentId != null &&
                           await _db.LetterDepartmentAccesses.AnyAsync(x => x.LetterId == letter.Id && x.DepartmentId == departmentId));
            if (!allowed)
            {
                return Forbid();
            }
        }
        letter.Classification = request.Classification;
        letter.Subject = request.Subject;
        letter.ExternalInstitutionId = request.ExternalInstitutionId;
        letter.Priority = request.Priority;
        letter.DueDate = request.DueDate;
        letter.OutgoingChannel = request.OutgoingChannel;
        letter.OutgoingDate = request.OutgoingDate;
        letter.OutgoingReference = request.OutgoingReference;
        letter.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var userId = GetUserId();
        if (userId.HasValue)
        {
            _db.DocumentHistories.Add(new DocumentHistory
            {
                LetterId = letter.Id,
                Action = "Updated",
                UserId = userId.Value
            });
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Letter", letter.Id, "Updated", userId.Value, letter.ProtocolNumber);
        }
        return Ok(letter);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, AssignLetterRequest request)
    {
        var letter = await _db.Letters.FindAsync(id);
        if (letter == null)
        {
            return NotFound();
        }
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }
        letter.AssignedToUserId = request.AssignedToUserId;
        letter.UpdatedAt = DateTime.UtcNow;
        _db.Assignments.Add(new Assignment
        {
            LetterId = letter.Id,
            AssignedByUserId = userId.Value,
            AssignedToUserId = request.AssignedToUserId,
            Note = request.Note
        });
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = letter.Id,
            Action = "Assigned",
            UserId = userId.Value,
            Note = request.Note
        });
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Letter", letter.Id, "Assigned", userId.Value, request.Note);
        await _notifications.CreateAsync(request.AssignedToUserId, "Assigned", $"Assigned: {letter.ProtocolNumber} - {letter.Subject}", letter.Id);
        return Ok(letter);
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var letter = await _db.Letters.FindAsync(id);
        if (letter == null)
        {
            return NotFound();
        }
        letter.Status = request.Status;
        letter.UpdatedAt = DateTime.UtcNow;
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = letter.Id,
            Action = "Status",
            UserId = GetUserId() ?? letter.CreatedByUserId,
            Note = request.Status.ToString()
        });
        await _db.SaveChangesAsync();
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await _audit.LogAsync("Letter", letter.Id, "Status", userId.Value, request.Status.ToString());
        }
        return Ok(letter);
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
