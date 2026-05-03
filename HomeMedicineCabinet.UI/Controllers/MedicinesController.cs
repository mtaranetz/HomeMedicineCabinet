using HomeMedicineCabinet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeMedicineCabinet.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace HomeMedicineCabinet.UI.Controllers;

[Authorize]
public class MedicinesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicinesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var medicines = await _context.Medicines
            .Include(m => m.Category)
            .Include(m => m.Stocks)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        return View(medicines);
    }

    //public async Task<IActionResult> Expiration()
    //{
    //    var today = DateTime.Today;
    //    var warningDate = today.AddDays(30);

    //    var stocks = await _context.MedicineStocks
    //        .Include(s => s.Medicine)
    //        .ThenInclude(m => m.Category)
    //        .Where(s => s.ExpirationDate <= warningDate)
    //        .OrderBy(s => s.ExpirationDate)
    //        .ToListAsync();

    //    return View(stocks);
    //}

    public async Task<IActionResult> LowStock()
    {
        var stocks = await _context.MedicineStocks
            .Include(s => s.Medicine)
            .ThenInclude(m => m.Category)
            .Where(s => s.Quantity <= s.MinQuantity)
            .OrderBy(s => s.Quantity)
            .ToListAsync();

        return View(stocks);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var medicine = await _context.Medicines
            .Include(m => m.Category)
            .Include(m => m.Stocks)
            .Include(m => m.IntakeSchedules)
            .ThenInclude(s => s.IntakeTimes)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medicine == null)
        {
            return NotFound();
        }

        return View(medicine);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(
            await _context.MedicineCategories.ToListAsync(),
            "Id",
            "Name"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "Id",
                "Name",
                model.CategoryId
            );

            return View(model);
        }

        var medicine = new Medicine
        {
            UserId = 1,
            CategoryId = model.CategoryId,
            Name = model.Name,
            Form = model.Form,
            Dosage = model.Dosage,
            Manufacturer = model.Manufacturer,
            Description = model.Description,
            CreatedAt = DateTime.Now,
            BaseUnit = model.BaseUnit
        };

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);

        if (medicine == null)
        {
            return NotFound();
        }

        ViewBag.Categories = new SelectList(
            await _context.MedicineCategories.ToListAsync(),
            "Id",
            "Name",
            medicine.CategoryId
        );

        var model = new MedicineCreateViewModel
        {
            Name = medicine.Name,
            CategoryId = medicine.CategoryId,
            Form = medicine.Form,
            Dosage = medicine.Dosage,
            Manufacturer = medicine.Manufacturer,
            Description = medicine.Description,
            BaseUnit = medicine.BaseUnit,

        };

        ViewBag.MedicineId = medicine.Id;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MedicineCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.MedicineCategories.ToListAsync(),
                "Id",
                "Name",
                model.CategoryId
            );

            ViewBag.MedicineId = id;
            return View(model);
        }

        var medicine = await _context.Medicines.FindAsync(id);

        if (medicine == null)
        {
            return NotFound();
        }

        medicine.CategoryId = model.CategoryId;
        medicine.Name = model.Name;
        medicine.Form = model.Form;
        medicine.Dosage = model.Dosage;
        medicine.Manufacturer = model.Manufacturer;
        medicine.Description = model.Description;
        medicine.UpdatedAt = DateTime.Now;
        medicine.BaseUnit = model.BaseUnit;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);

        if (medicine == null)
        {
            return NotFound();
        }

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}