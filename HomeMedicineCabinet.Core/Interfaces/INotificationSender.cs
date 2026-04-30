namespace HomeMedicineCabinet.Core.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string title, string message, int notificationId, int? intakeLogId = null);
}