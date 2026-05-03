using HomeMedicineCabinet.Core.Constants;
using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Core.Interfaces;
using HomeMedicineCabinet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeMedicineCabinet.Infrastructure.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationSender _sender;
    private readonly PushNotificationService _pushNotificationService;

    public NotificationService(
        ApplicationDbContext context,
        INotificationSender sender,
        PushNotificationService pushNotificationService)
    {
        _context = context;
        _sender = sender;
        _pushNotificationService = pushNotificationService;
    }

    public async Task CheckAllNotifications()
    {
        await CheckIntakeReminders();
        await CheckExpirationReminders();
        await CheckLowStockReminders();
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
                l.PlannedDateTime <= to &&
                !_context.Notifications.Any(n =>
                    n.IntakeLogId == l.Id &&
                    n.Type == NotificationTypes.IntakeReminder &&
                    n.SentAt != null))
            .ToListAsync();

        foreach (var log in logs)
        {
            var medicine = log.IntakeSchedule.Medicine;

            var title = "Пора принять лекарство";
            var message = $"{medicine.Name}, дозировка: {log.IntakeSchedule.Dose}";

            var notification = new Notification
            {
                UserId = log.IntakeSchedule.UserId,
                MedicineId = medicine.Id,
                IntakeLogId = log.Id,
                Type = NotificationTypes.IntakeReminder,
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

            await _pushNotificationService.SendAsync(
                title,
                message,
                "/Notifications",
                log.Id
            );
        }
    }

    public async Task CheckExpirationReminders()
    {
        var now = DateTime.Now;
        var today = DateTime.Today;
        var limitDate = today.AddDays(7);

        var stocks = await _context.MedicineStocks
            .Include(s => s.Medicine)
            .Where(s =>
                s.ExpirationDate >= today &&
                s.ExpirationDate <= limitDate &&
                !_context.Notifications.Any(n =>
                    n.MedicineId == s.MedicineId &&
                    n.Type == NotificationTypes.ExpirationReminder &&
                    n.ScheduledAt.Date == s.ExpirationDate.Date))
            .ToListAsync();

        foreach (var stock in stocks)
        {
            var daysLeft = (stock.ExpirationDate.Date - today).Days;

            var title = "Скоро истечёт срок годности";
            var message = daysLeft == 0
                ? $"Срок годности препарата {stock.Medicine.Name} истекает сегодня."
                : $"Срок годности препарата {stock.Medicine.Name} истекает через {daysLeft} дн.";

            var notification = new Notification
            {
                UserId = stock.Medicine.UserId,
                MedicineId = stock.MedicineId,
                Type = NotificationTypes.ExpirationReminder,
                Title = title,
                Message = message,
                ScheduledAt = stock.ExpirationDate,
                SentAt = now,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _pushNotificationService.SendAsync(
                title,
                message,
                "/Notifications"
            );
        }
    }

    public async Task CheckLowStockReminders()
    {
        var now = DateTime.Now;

        var stocks = await _context.MedicineStocks
            .Include(s => s.Medicine)
            .Where(s =>
                s.Quantity <= s.MinQuantity &&
                !_context.Notifications.Any(n =>
                    n.MedicineId == s.MedicineId &&
                    n.Type == NotificationTypes.LowStockReminder &&
                    n.CreatedAt.Date == DateTime.Today))
            .ToListAsync();

        foreach (var stock in stocks)
        {
            var title = "Заканчивается лекарство";
            var message = $"Осталось {stock.Quantity} {stock.Unit} препарата {stock.Medicine.Name}. Рекомендуется пополнить запас.";

            var notification = new Notification
            {
                UserId = stock.Medicine.UserId,
                MedicineId = stock.MedicineId,
                Type = NotificationTypes.LowStockReminder,
                Title = title,
                Message = message,
                ScheduledAt = now,
                SentAt = now,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _pushNotificationService.SendAsync(
                title,
                message,
                "/Notifications"
            );
        }
    }

    public async Task CheckLowStockForMedicine(int medicineId)
    {
        var now = DateTime.Now;

        var stock = await _context.MedicineStocks
            .Include(s => s.Medicine)
            .FirstOrDefaultAsync(s => s.MedicineId == medicineId);

        if (stock == null)
        {
            return;
        }

        if (stock.Quantity > stock.MinQuantity)
        {
            return;
        }

        var alreadyNotifiedToday = await _context.Notifications.AnyAsync(n =>
            n.MedicineId == medicineId &&
            n.Type == NotificationTypes.LowStockReminder &&
            n.CreatedAt.Date == DateTime.Today);

        if (alreadyNotifiedToday)
        {
            return;
        }

        var title = "Заканчивается лекарство";
        var message = $"Осталось {stock.Quantity} {stock.Unit} препарата {stock.Medicine.Name}. Рекомендуется пополнить запас.";

        var notification = new Notification
        {
            UserId = stock.Medicine.UserId,
            MedicineId = stock.MedicineId,
            Type = NotificationTypes.LowStockReminder,
            Title = title,
            Message = message,
            ScheduledAt = now,
            SentAt = now,
            IsRead = false,
            CreatedAt = now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _sender.SendAsync(
            title,
            message,
            notification.Id
        );

        await _pushNotificationService.SendAsync(
            title,
            message,
            "/Notifications"
        );
    }
}