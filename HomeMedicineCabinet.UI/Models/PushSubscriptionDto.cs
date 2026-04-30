namespace HomeMedicineCabinet.UI.Models;

public class PushSubscriptionDto
{
    public string Endpoint { get; set; } = string.Empty;

    public PushKeysDto Keys { get; set; } = new();
}

public class PushKeysDto
{
    public string P256dh { get; set; } = string.Empty;

    public string Auth { get; set; } = string.Empty;
}