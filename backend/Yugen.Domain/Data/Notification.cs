namespace Yugen.Domain.Data;

public class Notification
{
    public int id { get; set; }
    public long time { get; set; }
    public required string eventName { get; set; }

    public string? title { get; set; }
    public string? reason { get; set; }
    public string? icon { get; set; }
    public string? bannerIcon { get; set; }

    public bool hasBeenSeen { get; set; }

    public string? url { get; set; }
}
