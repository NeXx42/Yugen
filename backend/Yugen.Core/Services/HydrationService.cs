using System.Formats.Asn1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.AniList;
using Yugen.Providers.IdsMoe;

namespace Yugen.Core.Services;

public class HydrationService
{
    private readonly YugenContext _db;
    private readonly ILinkingProvider _linkingProvider;
    private readonly IMetaDataProvider _metaDataProvider;

    public HydrationService(YugenContext db, IOptions<ProviderConfig> options)
    {
        _db = db;
        _linkingProvider = new IdMoeLinkingProvider(options.Value.idMoe_Url!, options.Value.idMoe_ApiKey!);
        _metaDataProvider = new AniListProvider();
    }

    public async Task<Model_Media[]> SaveMedia(ICollection<int> aniListId)
    {
        Model_Media[] media = await _metaDataProvider.GetMediaInfo(aniListId);
        await _db.AddRangeAsync(media);
        await _db.SaveChangesAsync();

        return media;
    }

    public async Task<Model_Media?> SaveMedia(int aniListId)
    {
        Model_Media[] media = await _metaDataProvider.GetMediaInfo([aniListId]);

        if (media.Length != 1)
            return null;


        await _db.AddAsync(media[0]);
        await _db.SaveChangesAsync();

        return media[0];
    }

    public async Task HydrateMedia(Model_Media media, Model_Link? links, bool forceRehydration = false)
    {
        if ((media.Hydrated ?? false) && !forceRehydration)
            return;

        media.Hydrated = true;
        await HydrateEpisodes(media, links, forceRehydration);
        await _db.SaveChangesAsync();
    }

    private async Task HydrateEpisodes(Model_Media media, Model_Link? links, bool _)
    {
        if (links?.mal_id == null)
            return;

        _db.RemoveRange(_db.mediaEpisodes.Where(e => e.MediaId == media.Id));
        Model_MediaEpisode[] episodes = await _metaDataProvider.GetEpisodeData(links!.mal_id.Value);

        foreach (Model_MediaEpisode ep in episodes)
            ep.MediaId = media.Id;

        await _db.AddRangeAsync(episodes);
    }
}