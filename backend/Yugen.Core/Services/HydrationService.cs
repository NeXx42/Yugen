using Microsoft.EntityFrameworkCore;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Core.Services;

public class HydrationService(YugenContext _db, MetadataService _metadataProvider)
{
    public async Task<Model_Media[]> SaveMedia(ICollection<int> mediaIds)
    {
        List<Model_Media> results = await _db.media.Where(m => mediaIds.Contains(m.Id)).ToListAsync();
        Dictionary<int, Model_Media> newMedia = (await _metadataProvider.GetMediaInfo(mediaIds)).ToDictionary(m => m.Id, m => m);

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

    public async Task<Model_Media?> SaveMedia(int mediaId) => (await SaveMedia([mediaId]))[0];

    public async Task HydrateEpisodes(Model_Media media, bool clearOld)
    {
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

        Model_MediaEpisode[] providedEpisodes = await _metadataProvider.GetEpisodeData(media.Id);
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

        Dictionary<int, long?> results = await _metadataProvider.GetTimeOfNextEpisodes(mediaIds);
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