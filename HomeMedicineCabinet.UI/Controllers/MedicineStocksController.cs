using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HomeMedicineCabinet.UI.Controllers;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

[Authorize]
public class MedicineStocksController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicineStocksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int medicineId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == medicineId && m.UserId == userId);

        if (medicine == null)
        {
            return NotFound();
        }

        ViewBag.MedicineName = medicine.Name;
        ViewBag.BaseUnit = medicine.BaseUnit;

        return View(new MedicineStockCreateViewModel
        {
            MedicineId = medicineId,
            ExpirationDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicineStockCreateViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!ModelState.IsValid)
        {
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == model.MedicineId && m.UserId == userId);

            ViewBag.MedicineName = medicine?.Name;
            ViewBag.BaseUnit = medicine?.BaseUnit;
            return View(model);
        }

        var medicineCheck = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == model.MedicineId && m.UserId == userId);

        if (medicineCheck == null)
        {
            return NotFound();
        }

        var quantity = model.Quantity;

        if (model.IsPackage)
        {
            if (model.ItemsPerPackage == null || model.ItemsPerPackage <= 0)
            {
                ModelState.AddModelError(nameof(model.ItemsPerPackage), "Укажите количество единиц в упаковке");
                ViewBag.MedicineName = medicineCheck.Name;
                ViewBag.BaseUnit = medicineCheck.BaseUnit;
                return View(model);
            }

            quantity = model.Quantity * model.ItemsPerPackage.Value;
        }

        var stock = new MedicineStock
        {
            MedicineId = model.MedicineId,
            Quantity = quantity,
            Unit = medicineCheck.BaseUnit,
            MinQuantity = model.MinQuantity,
            ExpirationDate = model.ExpirationDate,
            StoragePlace = model.StoragePlace,
            UpdatedAt = DateTime.Now
        };

        _context.MedicineStocks.Add(stock);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Medicines");
    }
}