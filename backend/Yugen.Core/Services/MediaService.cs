using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Models.Library;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class MediaService
{
    private readonly IMediaProvider _mediaProvider;

    public MediaService(YugenContext db, IOptions<ProviderConfig> options)
    {
        _mediaProvider = new JellyfinMediaService(options.Value.jellyfin_Url!, options.Value.jellyfin_ApiKey!);
    }

    public async Task<string> Play()
    {
        return await _mediaProvider.Play();
    }

    public async Task<bool> LinkSonarrToJellyfin(Model_DownloadedMedia media)
    {
        string?[]? jellyfinIds = await _mediaProvider.MapPathToJellyfinId(media.downloadedEpisodes);

        if (jellyfinIds == null)
            return false;

        for (int i = 0; i < jellyfinIds.Length; i++)
            media.downloadedEpisodes.ElementAt(i).JellyfinId = jellyfinIds[i];

        return true;
    }
}
