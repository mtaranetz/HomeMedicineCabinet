using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeMedicineCabinet.Infrastructure.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationSender _sender;

    public NotificationService(
        ApplicationDbContext context,
        INotificationSender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task CheckIntakeReminders()
    {
        var now = DateTime.Now;
        var from = now.AddMinutes(-10);
        var to = now.AddMinutes(10);

        var logs = await _context.IntakeLogs
            .Include(l => l.IntakeSchedule)
            .ThenInclude(s => s.Medicine)
            .Where(l =>
                l.Status == "Planned" &&
                l.PlannedDateTime >= from &&
                l.PlannedDateTime <= to)
            .ToListAsync();

        foreach (var log in logs)
        {
            await _sender.SendAsync(
                "Напоминание",
                $"Пора принять: {log.IntakeSchedule.Medicine.Name} ({log.IntakeSchedule.Dose})"
            );
        }
    }
}