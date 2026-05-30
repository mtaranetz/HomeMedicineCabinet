using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HomeMedicineCabinet.UI.Models;

namespace HomeMedicineCabinet.UI.Controllers;

public class HomeController : Controller
{

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult About()
    {
        return View();
    }
}
