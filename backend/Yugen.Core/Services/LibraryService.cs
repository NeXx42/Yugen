using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;
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

    }

    public async Task<MediaCard[]> GetCurrentlyWatching(UserModel user)
    {
        return null;
    }

    public async Task RecheckDownloads(int aniListId)
    {
        Model_Media? media = await _db.media.Include(m => m.Episodes).FirstAsync(m => m.Id == aniListId);

        if (media == null)
            return;


    }
}
