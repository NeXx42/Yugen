using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Data;
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
}
