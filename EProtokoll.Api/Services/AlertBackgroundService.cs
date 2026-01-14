using EProtokoll.Api.Data;
using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EProtokoll.Api.Services;

public class AlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AlertOptions _options;

    public AlertBackgroundService(IServiceProvider serviceProvider, IOptions<AlertOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckOverdueAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }
    }

    private async Task CheckOverdueAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var overdueLetters = await db.Letters
            .Where(x => x.DueDate != null && x.DueDate < DateTime.UtcNow && x.Status != LetterStatus.Closed)
            .ToListAsync(token);

        foreach (var letter in overdueLetters)
        {
            var targetUserId = letter.AssignedToUserId ?? letter.CreatedByUserId;
            var message = $"Overdue: {letter.ProtocolNumber} - {letter.Subject}";
            await notifications.CreateAsync(targetUserId, "Overdue", message, letter.Id);
        }
    }
}
