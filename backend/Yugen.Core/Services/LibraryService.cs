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
    private readonly HydrationService _hydrationService;

    private readonly ILibraryProvider _libraryProvider;

    public LibraryService(YugenContext db, IOptions<ProviderConfig> options, HydrationService hydrationService)
    {
        _db = db;

        _hydrationService = hydrationService;
        _libraryProvider = new JellyfinLibraryService(options.Value.jellyfin_Url!, options.Value.jellyfin_ApiKey!);
    }

    public async Task ResyncLibrary(UserModel user)
    {

    }

    public async Task<MediaCard[]> GetCurrentlyWatching(UserModel user)
    {
        return null;
    }
}
