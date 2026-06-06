using Yugen.Core.Data;

namespace Yugen.Domain.Data;

public class Notification
{
    public int id { get; set; }
    public long time { get; set; }

    public required string eventName { get; set; }
    public string? source { get; set; }
    public string? reason { get; set; }

    public MediaCard? media { get; set; }

    public bool hasBeenSeen { get; set; }
    public string? url { get; set; }
}
