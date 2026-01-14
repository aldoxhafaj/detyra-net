using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Services;

public interface IReportService
{
    Task<SummaryReport> GetSummaryAsync(DateTime? from, DateTime? to);
    Task<List<OverdueReport>> GetOverdueAsync(DateTime? from, DateTime? to);
    Task<List<UserReport>> GetByUserAsync(DateTime? from, DateTime? to);
    Task<List<TrackingReport>> GetTrackingAsync(DateTime? from, DateTime? to);
    Task<List<PriorityReport>> GetByPriorityAsync(DateTime? from, DateTime? to);
    Task<List<StatusReport>> GetByStatusAsync(DateTime? from, DateTime? to);
    Task<List<DepartmentReport>> GetByDepartmentAsync(DateTime? from, DateTime? to);
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SummaryReport> GetSummaryAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to);
        var total = await query.CountAsync();
        var incoming = await query.CountAsync(x => x.Type == LetterType.Incoming);
        var outgoing = await query.CountAsync(x => x.Type == LetterType.Outgoing);
        var internalCount = await query.CountAsync(x => x.Type == LetterType.Internal);
        var publicCount = await query.CountAsync(x => x.Classification == DocumentClassification.Public);
        var restrictedCount = await query.CountAsync(x => x.Classification == DocumentClassification.Restricted);
        var secretCount = await query.CountAsync(x => x.Classification == DocumentClassification.Secret);
        return new SummaryReport(total, incoming, outgoing, internalCount, publicCount, restrictedCount, secretCount);
    }

    public async Task<List<OverdueReport>> GetOverdueAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to)
            .Where(x => x.DueDate != null && x.DueDate < DateTime.UtcNow && x.Status != LetterStatus.Closed);

        return await query.Select(x => new OverdueReport(
            x.Id,
            x.ProtocolNumber,
            x.Subject,
            (int)Math.Ceiling((DateTime.UtcNow - x.DueDate!.Value).TotalDays),
            x.AssignedToUserId))
            .ToListAsync();
    }

    public async Task<List<UserReport>> GetByUserAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to);
        return await query
            .GroupBy(x => x.CreatedByUserId)
            .Select(g => new UserReport(g.Key, g.Count()))
            .ToListAsync();
    }

    public async Task<List<TrackingReport>> GetTrackingAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to)
            .Where(x => x.DueDate != null);

        var items = await query
            .Select(x => new
            {
                x.Id,
                x.ProtocolNumber,
                x.Subject,
                x.DueDate
            })
            .ToListAsync();

        var results = new List<TrackingReport>();
        foreach (var letter in items)
        {
            var last = await _db.DocumentHistories
                .Where(x => x.LetterId == letter.Id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
            var lastAction = last?.Action ?? "None";
            var lastAt = last?.CreatedAt ?? DateTime.UtcNow;
            var daysOverdue = letter.DueDate < DateTime.UtcNow
                ? (int)Math.Ceiling((DateTime.UtcNow - letter.DueDate!.Value).TotalDays)
                : 0;
            results.Add(new TrackingReport(letter.Id, letter.ProtocolNumber, letter.Subject, lastAction, lastAt, daysOverdue));
        }
        return results;
    }

    public async Task<List<PriorityReport>> GetByPriorityAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to);
        return await query
            .GroupBy(x => x.Priority)
            .Select(g => new PriorityReport(g.Key.ToString(), g.Count()))
            .ToListAsync();
    }

    public async Task<List<StatusReport>> GetByStatusAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to);
        return await query
            .GroupBy(x => x.Status)
            .Select(g => new StatusReport(g.Key.ToString(), g.Count()))
            .ToListAsync();
    }

    public async Task<List<DepartmentReport>> GetByDepartmentAsync(DateTime? from, DateTime? to)
    {
        var query = FilterByDate(_db.Letters.AsQueryable(), from, to);
        return await query
            .Join(_db.Users, l => l.CreatedByUserId, u => u.Id, (l, u) => new { u.DepartmentId })
            .GroupBy(x => x.DepartmentId)
            .Select(g => new DepartmentReport(g.Key, g.Count()))
            .ToListAsync();
    }

    private static IQueryable<Letter> FilterByDate(IQueryable<Letter> query, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }
        return query;
    }
}
