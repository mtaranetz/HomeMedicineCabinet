using HomeMedicineCabinet.UI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HomeMedicineCabinet.UI.Controllers;

public class TestNotificationsController : Controller
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public TestNotificationsController(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Send()
    {
        await _hubContext.Clients.All.SendAsync(
            "ReceiveNotification",
            "Тестовое уведомление",
            "SignalR успешно работает!"
        );

        return RedirectToAction("Index", "Medicines");
    }
}