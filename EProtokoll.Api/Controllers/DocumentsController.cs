using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using EProtokoll.Api.Services;
using EProtokoll.Api.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;
    private readonly IAuditService _audit;

    public DocumentsController(AppDbContext db, IStorageService storage, IAuditService audit)
    {
        _db = db;
        _storage = storage;
        _audit = audit;
    }

    [HttpPost("letters/{id:int}/documents")]
    public async Task<IActionResult> Upload(int id, IFormFile file)
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
        await using var stream = file.OpenReadStream();
        var encrypt = letter.Classification == DocumentClassification.Secret;
        var result = await _storage.SaveAsync(stream, encrypt);
        var document = new Document
        {
            LetterId = id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = result.size,
            HashSha256 = result.hash,
            StorageKey = result.storageKey,
            IsEncrypted = result.isEncrypted
        };
        _db.Documents.Add(document);
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = letter.Id,
            Action = "Uploaded",
            UserId = letter.CreatedByUserId,
            Note = document.FileName
        });
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Document", document.Id, "Uploaded", letter.CreatedByUserId, document.FileName);
        return Ok(new DocumentResponse(document.Id, document.FileName, document.ContentType, document.SizeBytes, document.IsEncrypted, document.IsScanned));
    }

    [HttpGet("letters/{id:int}/documents")]
    public async Task<IActionResult> GetForLetter(int id)
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
        var docs = await _db.Documents.Where(x => x.LetterId == id).ToListAsync();
        var result = docs.Select(x => new DocumentResponse(x.Id, x.FileName, x.ContentType, x.SizeBytes, x.IsEncrypted, x.IsScanned));
        return Ok(result);
    }

    [HttpGet("documents/{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _db.Documents.Include(x => x.Letter).FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null)
        {
            return NotFound();
        }
        if (doc.Letter != null && doc.Letter.Classification == DocumentClassification.Secret && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            return Forbid();
        }
        if (doc.Letter != null && doc.Letter.Classification == DocumentClassification.Restricted && !User.IsInRole("Manager") && !User.IsInRole("Administrator"))
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Forbid();
            }
            var departmentId = await GetUserDepartmentIdAsync(userId.Value);
            var allowed = doc.Letter.CreatedByUserId == userId || doc.Letter.AssignedToUserId == userId ||
                          await _db.LetterAccesses.AnyAsync(x => x.LetterId == doc.Letter.Id && x.UserId == userId) ||
                          (departmentId != null &&
                           await _db.LetterDepartmentAccesses.AnyAsync(x => x.LetterId == doc.Letter.Id && x.DepartmentId == departmentId));
            if (!allowed)
            {
                return Forbid();
            }
        }
        var stream = await _storage.OpenReadAsync(doc.StorageKey, doc.IsEncrypted);
        if (stream == null)
        {
            return NotFound();
        }
        if (doc.Letter != null)
        {
            var userId = GetUserId() ?? doc.Letter.CreatedByUserId;
            _db.DocumentHistories.Add(new DocumentHistory
            {
                LetterId = doc.Letter.Id,
                Action = "Downloaded",
                UserId = userId,
                Note = doc.FileName
            });
            await _db.SaveChangesAsync();
        }
        return File(stream, doc.ContentType, doc.FileName);
    }

    [HttpDelete("documents/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null)
        {
            return NotFound();
        }
        var storageKey = doc.StorageKey;
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        var stillUsed = await _db.Documents.AnyAsync(x => x.StorageKey == storageKey);
        if (!stillUsed)
        {
            await _storage.DeleteAsync(storageKey);
        }
        return NoContent();
    }

    [HttpPost("letters/{id:int}/scan")]
    public async Task<IActionResult> Scan(int id, IFormFile file)
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
        await using var stream = file.OpenReadStream();
        var encrypt = letter.Classification == DocumentClassification.Secret;
        var result = await _storage.SaveAsync(stream, encrypt);
        var document = new Document
        {
            LetterId = id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = result.size,
            HashSha256 = result.hash,
            StorageKey = result.storageKey,
            IsEncrypted = result.isEncrypted,
            IsScanned = true
        };
        _db.Documents.Add(document);
        _db.DocumentHistories.Add(new DocumentHistory
        {
            LetterId = letter.Id,
            Action = "Scanned",
            UserId = letter.CreatedByUserId,
            Note = document.FileName
        });
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Document", document.Id, "Scanned", letter.CreatedByUserId, document.FileName);
        return Ok(new DocumentResponse(document.Id, document.FileName, document.ContentType, document.SizeBytes, document.IsEncrypted, document.IsScanned));
    }

    private int? GetUserId()
    {
        string? id = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirstValue(ClaimTypes.Name);
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
