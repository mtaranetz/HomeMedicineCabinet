using HomeMedicineCabinet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HomeMedicineCabinet.UI.Controllers;

[Authorize]
public class ExpirationController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpirationController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var warningDate = today.AddDays(7);
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var stocks = await _context.MedicineStocks
            .Include(s => s.Medicine)
            .ThenInclude(m => m.Category)
            .OrderBy(s => s.ExpirationDate)
            .Where(m => m.Medicine.UserId == userId)
            .ToListAsync();

        ViewBag.Today = today;
        ViewBag.WarningDate = warningDate;

        return View(stocks);
    }
}   