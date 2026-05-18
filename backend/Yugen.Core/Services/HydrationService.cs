using System.Formats.Asn1;
using EFCore.BulkExtensions;
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
    private readonly IMetaDataProvider _metaDataProvider;

    public HydrationService(YugenContext db, SettingsCache settings)
    {
        _db = db;
        _metaDataProvider = new AniListProvider();
    }

    public async Task<Model_Media[]> SaveMedia(ICollection<int> aniListId)
    {
        Model_Media[] media = await _metaDataProvider.GetMediaInfo(aniListId);
        await _db.BulkInsertOrUpdateAsync(media);

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

    public async Task<bool> HydrateMedia(Model_Media media, Model_Link? links, bool forceRehydration = false)
    {
        if ((media.Hydrated ?? false) && !forceRehydration)
            return true;

        media.Hydrated = true;
        try
        {
            await HydrateEpisodes(media, links, forceRehydration);
            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine($"Failed to hydrate - {media.Id}");

            return false;
        }
    }

    private async Task HydrateEpisodes(Model_Media media, Model_Link? links, bool _)
    {
        if (links?.mal_id == null)
            return;

        Model_MediaEpisode[] providedEpisodes = await _metaDataProvider.GetEpisodeData(links!.mal_id.Value);
        Model_MediaEpisode[] existingEpisodes = await _db.mediaEpisodes.Where(e => e.MediaId == media.Id).ToArrayAsync();

        List<Model_MediaEpisode> toAdd = [.. providedEpisodes];

        foreach (Model_MediaEpisode existingEpisode in existingEpisodes)
        {
            Model_MediaEpisode? providedEpisode = providedEpisodes.FirstOrDefault(e => e.EpisodeNumber == existingEpisode.EpisodeNumber);

            if (providedEpisode != null)
            {
                toAdd.Remove(providedEpisode);

                existingEpisode.IsFiller = providedEpisode.IsFiller;
                existingEpisode.IsRecap = providedEpisode.IsRecap;
                existingEpisode.Score = providedEpisode.Score;
            }
        }

        foreach (Model_MediaEpisode newEp in providedEpisodes)
            newEp.MediaId = media.Id;

        await _db.AddRangeAsync(toAdd);
        await _db.SaveChangesAsync();
    }
}