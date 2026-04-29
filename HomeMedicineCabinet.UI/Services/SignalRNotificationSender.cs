using HomeMedicineCabinet.Core.Interfaces;
using HomeMedicineCabinet.UI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HomeMedicineCabinet.UI.Services;

public class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotificationSender(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task SendAsync(string title, string message)
    {
        await _hub.Clients.All.SendAsync("ReceiveNotification", title, message);
    }
}