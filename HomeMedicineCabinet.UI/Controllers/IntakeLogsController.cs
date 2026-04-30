using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeMedicineCabinet.UI.Controllers;

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

        log.Status = status;

        if (status == "Taken" || status == "Skipped")
        {
            log.ActualDateTime = DateTime.Now;
        }
        else
        {
            log.ActualDateTime = null;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        await GenerateTodayLogs();
        //await _notificationService.CheckIntakeReminders();

        var today = DateTime.Today;

        var logs = await _context.IntakeLogs
            .Include(l => l.IntakeSchedule)
            .ThenInclude(s => s.Medicine)
            .Where(l => l.PlannedDateTime.Date == today)
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

        log.Status = "Taken";
        log.ActualDateTime = DateTime.Now;

        await _context.SaveChangesAsync();

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
}