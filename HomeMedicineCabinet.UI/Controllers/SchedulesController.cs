using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HomeMedicineCabinet.UI.Controllers;

public class SchedulesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SchedulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var schedules = await _context.IntakeSchedules
            .Include(s => s.Medicine)
            .Include(s => s.IntakeTimes)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return View(schedules);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadMedicines();

        return View(new ScheduleCreateViewModel
        {
            StartDate = DateTime.Today,
            FrequencyType = "Daily",
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadMedicines(model.MedicineId);
            return View(model);
        }

        var schedule = new IntakeSchedule
        {
            MedicineId = model.MedicineId,
            UserId = 1,
            Dose = model.Dose,
            FrequencyType = model.FrequencyType,
            TimesPerDay = model.TimesPerDay,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive,
            Comment = model.Comment,
            CreatedAt = DateTime.Now
        };

        var times = model.IntakeTimesText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var timeText in times)
        {
            if (TimeSpan.TryParse(timeText, out var time))
            {
                schedule.IntakeTimes.Add(new IntakeTime
                {
                    IntakeTimeValue = time
                });
            }
        }

        if (!schedule.IntakeTimes.Any())
        {
            ModelState.AddModelError(nameof(model.IntakeTimesText), "Укажите хотя бы одно корректное время приёма.");
            await LoadMedicines(model.MedicineId);
            return View(model);
        }

        _context.IntakeSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadMedicines(int? selectedMedicineId = null)
    {
        ViewBag.Medicines = new SelectList(
            await _context.Medicines.OrderBy(m => m.Name).ToListAsync(),
            "Id",
            "Name",
            selectedMedicineId
        );
    }
}