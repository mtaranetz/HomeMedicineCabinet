using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeMedicineCabinet.UI.Controllers;

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
        var medicine = await _context.Medicines.FindAsync(medicineId);

        if (medicine == null)
        {
            return NotFound();
        }

        ViewBag.MedicineName = medicine.Name;

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
        if (!ModelState.IsValid)
        {
            var medicine = await _context.Medicines.FindAsync(model.MedicineId);
            ViewBag.MedicineName = medicine?.Name;
            return View(model);
        }

        var stock = new MedicineStock
        {
            MedicineId = model.MedicineId,
            Quantity = model.Quantity,
            Unit = model.Unit,
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