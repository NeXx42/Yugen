using Microsoft.EntityFrameworkCore;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.AniList;

namespace Yugen.Core.Services;

public class HydrationService
{
    private readonly YugenContext _db;
    private readonly IMetaDataProvider _metaDataProvider;

    public HydrationService(YugenContext db, SettingsCache settings, ILogging logger)
    {
        _db = db;
        _metaDataProvider = new AniListProvider(logger);
    }

    public async Task<Model_Media[]> SaveMedia(ICollection<int> aniListId, MediaSearchQuery? req = null)
    {
        req ??= new MediaSearchQuery();
        req.ids = aniListId;

        List<Model_Media> results = await _db.media.Where(m => aniListId.Contains(m.Id)).ToListAsync();
        Dictionary<int, Model_Media> newMedia = (await _metaDataProvider.GetMediaInfo(req)).ToDictionary(m => m.Id, m => m);

        foreach (Model_Media existing in results)
        {
            if (newMedia.TryGetValue(existing.Id, out Model_Media? fresh))
            {
                existing.Update(fresh);
                newMedia.Remove(existing.Id);
            }
        }

        results.AddRange(newMedia.Values);

        await _db.AddRangeAsync(newMedia.Values);
        await _db.SaveChangesAsync();

        return results.ToArray();
    }

    public async Task<Model_Media?> SaveMedia(int aniListId, MediaSearchQuery? req = null) => (await SaveMedia([aniListId], req))[0];

    public async Task HydrateEpisodes(Model_Media media, bool clearOld)
    {
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == media.Id);
        link ??= Model_Link.Fake(media.Id);

        Model_MediaEpisode[] existingEpisodes = [];

        if (clearOld)
        {
            _db.RemoveRange(_db.mediaEpisodes.Where(e => e.MediaId == media.Id));
            await _db.SaveChangesAsync();
        }
        else
        {
            existingEpisodes = await _db.mediaEpisodes.Where(e => e.MediaId == media.Id).ToArrayAsync();
        }

        Model_MediaEpisode[] providedEpisodes = await _metaDataProvider.GetEpisodeData(link);
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

        foreach (Model_MediaEpisode newEp in toAdd)
            newEp.MediaId = media.Id;

        if (toAdd.Count > 0)
            await _db.AddRangeAsync(toAdd);

        //media.Hydrated = true;
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, long?>> HydrateReleaseDates(ICollection<int> mediaIds)
    {
        if (mediaIds.Count == 0)
            return new Dictionary<int, long?>();

        Dictionary<int, long?> results = await _metaDataProvider.GetTimeOfNextEpisodes(mediaIds);
        Model_Media[] mediaEntries = await _db.media.Where(m => mediaIds.Contains(m.Id)).ToArrayAsync();

        foreach (Model_Media media in mediaEntries)
        {
            if (results.TryGetValue(media.Id, out long? val))
                media.NextEpisodeReleaseDate = val;
        }

        await _db.SaveChangesAsync();
        return results;
    }
}