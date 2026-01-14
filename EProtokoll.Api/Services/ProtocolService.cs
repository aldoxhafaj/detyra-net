using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Services;

public interface IProtocolService
{
    Task<ProtocolBook> OpenBookAsync(int year);
    Task<bool> CloseBookAsync(int year);
    Task<string> NextProtocolNumberAsync(int year);
}

public class ProtocolService : IProtocolService
{
    private readonly AppDbContext _db;

    public ProtocolService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProtocolBook> OpenBookAsync(int year)
    {
        var existing = await _db.ProtocolBooks.SingleOrDefaultAsync(x => x.Year == year);
        if (existing != null)
        {
            existing.IsOpen = true;
            existing.ClosedAt = null;
            await _db.SaveChangesAsync();
            return existing;
        }

        var book = new ProtocolBook
        {
            Year = year,
            IsOpen = true
        };
        _db.ProtocolBooks.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<bool> CloseBookAsync(int year)
    {
        var book = await _db.ProtocolBooks.SingleOrDefaultAsync(x => x.Year == year);
        if (book == null)
        {
            return false;
        }

        book.IsOpen = false;
        book.ClosedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string> NextProtocolNumberAsync(int year)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var book = await _db.ProtocolBooks.SingleOrDefaultAsync(x => x.Year == year);
        if (book == null || !book.IsOpen)
        {
            throw new InvalidOperationException("Protocol book closed");
        }

        var counter = await _db.ProtocolCounters.SingleOrDefaultAsync(x => x.Year == year);
        if (counter == null)
        {
            counter = new ProtocolCounter { Year = year, CurrentValue = 0 };
            _db.ProtocolCounters.Add(counter);
        }

        counter.CurrentValue += 1;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return $"{year}-{counter.CurrentValue:D6}";
    }
}
