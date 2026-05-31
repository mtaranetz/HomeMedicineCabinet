using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HomeMedicineCabinet.UI.Controllers;

[Authorize]
public class SchedulesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SchedulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var schedules = await _context.IntakeSchedules
            .Include(s => s.Medicine)
            .Include(s => s.IntakeTimes)
            .OrderByDescending(s => s.CreatedAt)
            .Where(s => s.UserId == userId)
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
            FrequencyType = "Ежедневно",
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduleCreateViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // 1. Проверка, что MedicineId существует и принадлежит текущему пользователю
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == model.MedicineId && m.UserId == userId);
        if (medicine == null)
        {
            ModelState.AddModelError(nameof(model.MedicineId),
                "Выбранное лекарство не найдено или недоступно.");
        }

        // 2. Проверка EndDate >= StartDate (если дата окончания указана)
        if (model.EndDate.HasValue && model.EndDate.Value.Date < model.StartDate.Date)
        {
            ModelState.AddModelError(nameof(model.EndDate),
                "Дата окончания не может быть раньше даты начала.");
        }

        // Если уже есть ошибки валидации (включая атрибуты модели) – возвращаем форму
        if (!ModelState.IsValid)
        {
            await LoadMedicines(model.MedicineId);
            return View(model);
        }

        var schedule = new IntakeSchedule
        {
            MedicineId = model.MedicineId,
            UserId = userId,
            Dose = model.Dose,
            FrequencyType = model.FrequencyType,
            TimesPerDay = model.TimesPerDay,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive,
            Comment = model.Comment,
            CreatedAt = DateTime.Now
        };

        // Парсинг строки с временами приёма
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

        // 3. Проверка на дубликаты времени
        var distinctTimesCount = schedule.IntakeTimes
            .Select(t => t.IntakeTimeValue)
            .Distinct()
            .Count();
        if (distinctTimesCount != schedule.IntakeTimes.Count)
        {
            ModelState.AddModelError(nameof(model.IntakeTimesText),
                "Обнаружены дублирующиеся значения времени приёма.");
        }

        // 4. Ограничение максимального количества приёмов (например, не более 24)
        const int maxTimesPerDay = 24;
        if (schedule.IntakeTimes.Count > maxTimesPerDay)
        {
            ModelState.AddModelError(nameof(model.IntakeTimesText),
                $"Количество приёмов не должно превышать {maxTimesPerDay}.");
        }

        // 5. Сопоставление TimesPerDay и количества переданных времён
        //    Если частота "Ежедневно", ожидаем точное совпадение
        if (model.FrequencyType == "Ежедневно" &&
            schedule.IntakeTimes.Count != model.TimesPerDay)
        {
            ModelState.AddModelError(nameof(model.IntakeTimesText),
                $"Количество указанных времён ({schedule.IntakeTimes.Count}) " +
                $"не соответствует заданному числу приёмов в день ({model.TimesPerDay}).");
        }

        // 6. Хотя бы одно корректное время (оригинальная проверка)
        if (!schedule.IntakeTimes.Any())
        {
            ModelState.AddModelError(nameof(model.IntakeTimesText),
                "Укажите хотя бы одно корректное время приёма.");
        }

        // Финальная проверка после всех добавленных ошибок
        if (!ModelState.IsValid)
        {
            await LoadMedicines(model.MedicineId);
            return View(model);
        }

        _context.IntakeSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadMedicines(int? selectedMedicineId = null)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        ViewBag.Medicines = new SelectList(
            await _context.Medicines
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Name)
                .ToListAsync(),
            "Id",
            "Name",
            selectedMedicineId
        );
    }
}