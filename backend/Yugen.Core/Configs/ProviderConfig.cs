namespace Yugen.Core.Configs;

public class ProviderConfig
{
    public string? jellyfin_Url { set; get; } = null;
    public string? jellyfin_ApiKey { set; get; } = null;

    public string? idMoe_Url { set; get; } = null;
    public string? idMoe_ApiKey { set; get; } = null;

    public string? jikan_Url { set; get; } = null;

    public string? sonarr_Url { get; set; } = null;
    public string? sonarr_ApiKey { get; set; } = null;
}
