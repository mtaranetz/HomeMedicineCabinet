namespace HomeMedicineCabinet.Core.Entities;

public class PushSubscription
{
    public int Id { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string P256dh { get; set; } = string.Empty;

    public string Auth { get; set; } = string.Empty;

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int UserId { get; set; }

    public User? User { get; set; }
}