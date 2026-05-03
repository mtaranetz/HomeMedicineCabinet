using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeMedicineCabinet.UI.Controllers;

[Authorize]
public class IntakeLogsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notificationService;

    public IntakeLogsController(
    ApplicationDbContext context,
    NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string status)
    {
        var log = await _context.IntakeLogs.FindAsync(id);

        if (log == null)
        {
            return NotFound();
        }

        if (status != "Planned" && status != "Taken" && status != "Skipped")
        {
            return BadRequest();
        }

        var previousStatus = log.Status;

        log.Status = status;

        if (status == "Taken" || status == "Skipped")
        {
            log.ActualDateTime = DateTime.Now;
        }
        else
        {
            log.ActualDateTime = null;
        }

        if (previousStatus != "Taken" && status == "Taken")
        {
            await DecreaseMedicineStockForIntakeLog(log);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        await GenerateTodayLogs();
        //await _notificationService.CheckIntakeReminders();
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);


        var today = DateTime.Today;

        var logs = await _context.IntakeLogs
            .Include(l => l.IntakeSchedule)
            .ThenInclude(s => s.Medicine)
            .Where(l => l.PlannedDateTime.Date == today && l.IntakeSchedule.UserId == userId)
            .OrderBy(l => l.PlannedDateTime)
            .ToListAsync();

        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsTaken(int id)
    {
        var log = await _context.IntakeLogs.FindAsync(id);

        if (log == null)
        {
            return NotFound();
        }

        if (log.Status != "Taken")
        {
            log.Status = "Taken";
            log.ActualDateTime = DateTime.Now;

            await DecreaseMedicineStockForIntakeLog(log);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsSkipped(int id)
    {
        var log = await _context.IntakeLogs.FindAsync(id);

        if (log == null)
        {
            return NotFound();
        }

        log.Status = "Skipped";
        log.ActualDateTime = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetStatusFromNotification(int id, string status)
    {
        if (status != "Taken" && status != "Skipped")
        {
            return BadRequest();
        }

        var log = await _context.IntakeLogs.FindAsync(id);

        if (log == null)
        {
            return NotFound();
        }

        if (log.Status != "Planned")
        {
            return Ok();
        }

        log.Status = status;
        log.ActualDateTime = DateTime.Now;

        if (status == "Taken")
        {
            await DecreaseMedicineStockForIntakeLog(log);
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task GenerateTodayLogs()
    {
        var today = DateTime.Today;

        var schedules = await _context.IntakeSchedules
            .Include(s => s.IntakeTimes)
            .Where(s =>
                s.IsActive &&
                s.StartDate.Date <= today &&
                (s.EndDate == null || s.EndDate.Value.Date >= today))
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            foreach (var intakeTime in schedule.IntakeTimes)
            {
                var plannedDateTime = today.Add(intakeTime.IntakeTimeValue);

                var exists = await _context.IntakeLogs.AnyAsync(l =>
                    l.IntakeScheduleId == schedule.Id &&
                    l.PlannedDateTime == plannedDateTime);

                if (!exists)
                {
                    _context.IntakeLogs.Add(new IntakeLog
                    {
                        IntakeScheduleId = schedule.Id,
                        PlannedDateTime = plannedDateTime,
                        Status = "Planned"
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task DecreaseMedicineStockForIntakeLog(IntakeLog log)
    {
        var logWithSchedule = await _context.IntakeLogs
            .Include(l => l.IntakeSchedule)
            .FirstOrDefaultAsync(l => l.Id == log.Id);

        if (logWithSchedule?.IntakeSchedule == null)
        {
            return;
        }

        var medicineId = logWithSchedule.IntakeSchedule.MedicineId;

        var stock = await _context.MedicineStocks
            .FirstOrDefaultAsync(s => s.MedicineId == medicineId);

        if (stock == null)
        {
            return;
        }

        var doseString = logWithSchedule.IntakeSchedule.Dose;

        decimal dose = 1;

        if (!string.IsNullOrWhiteSpace(doseString))
        {
            var match = Regex.Match(doseString.Replace(',', '.'), @"\d+(\.\d+)?");

            if (match.Success &&
                decimal.TryParse(
                    match.Value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsedDose))
            {
                dose = parsedDose;
            }
        }

        if (dose <= 0)
        {
            dose = 1;
        }

        if (dose <= 0)
        {
            dose = 1;
        }

        stock.Quantity -= dose;

        if (stock.Quantity < 0)
        {
            stock.Quantity = 0;
        }

        stock.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        await _notificationService.CheckLowStockForMedicine(medicineId);
    }
}