using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Models;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class LibraryService
{
    private readonly YugenContext _db;
    private readonly ILibraryProvider _libraryProvider;

    public LibraryService(YugenContext db, IOptions<ProviderConfig> options)
    {
        _db = db;
        _libraryProvider = new JellyfinLibraryService(options.Value.jellyfin_Url!, options.Value.jellyfin_ApiKey!);
    }

    public async Task ResyncLibrary(UserModel user)
    {
        var externalTruth = await _libraryProvider.GetExternalMedia(user.ProviderId);
        var externalIds = externalTruth.Where(e => e.id.HasValue).Select(e => e.id!.Value).ToHashSet();

        var existing = await _db.media.Where(x => externalIds.Contains(x.Id)).ToArrayAsync();

        List<MediaModel> newEntries = new List<MediaModel>();

        foreach (var newEntry in externalTruth)
        {
            if (existing.Any(x => x.Id == newEntry.id))
                continue;

            newEntries.Add(new MediaModel()
            {
                Id = newEntry.id!.Value,
                Title = newEntry.title,
                externalProviders = [ new MediaExternalProviderModel(){
                    MediaId = newEntry.id!.Value,
                    ExternalIdentity = newEntry.aniDb!,
                    ProviderType = Domain.Enums.ProviderType.AniList
                }]
            });
        }

        await _db.AddRangeAsync(newEntries);
        await _db.SaveChangesAsync();
    }

    public async Task<MediaCard[]> GetCurrentlyWatching(UserModel user)
    {
        MediaModel[] cards = await _db.media.Take(10).ToArrayAsync();
        return cards.Select(x => new MediaCard()
        {
            Id = x.Id,
            Title = x.Title,
        }).ToArray();
    }
}
