using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using HomeMedicineCabinet.Core.Entities;

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

        Console.WriteLine($"[Notifications] Check started: {DateTime.Now}");

        var allPlanned = await _context.IntakeLogs
            .Where(l => l.Status == "Planned")
            .CountAsync();

        Console.WriteLine($"[Notifications] Planned logs: {allPlanned}");
        Console.WriteLine($"[Notifications] Window: {from} - {to}");

        var logs = await _context.IntakeLogs
            .Include(l => l.IntakeSchedule)
            .ThenInclude(s => s.Medicine)
            .Where(l =>
                l.Status == "Planned" &&
                l.PlannedDateTime >= from &&
                l.PlannedDateTime <= to &&
                !_context.Notifications.Any(n =>
                    n.IntakeLogId == l.Id &&
                    n.Type == "IntakeReminder" &&
                    n.SentAt != null))
            .ToListAsync();

        foreach (var log in logs)
        {
            var title = "Напоминание";
            var message = $"Пора принять: {log.IntakeSchedule.Medicine.Name} ({log.IntakeSchedule.Dose})";
            Console.WriteLine($"[Notifications] Sending log id: {log.Id}");
            var notification = new Notification
            {
                UserId = log.IntakeSchedule.UserId,
                MedicineId = log.IntakeSchedule.MedicineId,
                IntakeLogId = log.Id,
                Type = "IntakeReminder",
                Title = title,
                Message = message,
                ScheduledAt = log.PlannedDateTime,
                SentAt = now,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _sender.SendAsync(
                title,
                message,
                notification.Id,
                log.Id
            );
        }
    }
}