namespace HomeMedicineCabinet.Core.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string title, string message);
}