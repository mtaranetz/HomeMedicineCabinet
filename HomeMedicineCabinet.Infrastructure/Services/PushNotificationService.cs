using System.Text.Json;
using HomeMedicineCabinet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebPush;

namespace HomeMedicineCabinet.Infrastructure.Services;

public class PushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PushNotificationService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task SendAsync(string title,
                                string message,
                                string url = "/Notifications",
                                int? intakeLogId = null)
    {
        var publicKey = _configuration["Vapid:PublicKey"];
        var privateKey = _configuration["Vapid:PrivateKey"];
        var subject = _configuration["Vapid:Subject"];

        var subscriptions = await _context.PushSubscriptions.ToListAsync();

        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var webPushClient = new WebPushClient();

        var payload = JsonSerializer.Serialize(new
        {
            title,
            message,
            url,
            intakeLogId
        });

        foreach (var sub in subscriptions)
        {
            var pushSub = new WebPush.PushSubscription(
                sub.Endpoint,
                sub.P256dh,
                sub.Auth
            );

            try
            {
                await webPushClient.SendNotificationAsync(pushSub, payload, vapidDetails);
            }
            catch (WebPushException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                    ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _context.PushSubscriptions.Remove(sub);
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}   