using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.AniList;
using Yugen.Providers.Tenrai;

namespace Yugen.Core.Services;

public class MetadataService
{
    private readonly YugenContext _db;
    private readonly IMetaDataProvider _provider;

    private readonly PropertyInfo _linkProperty;

    private readonly string _linkMediaIdColumnName;
    private readonly string _linkLinkKeyColumnName;
    private readonly string _linkLinkTableName;

    public MetadataService(YugenContext db, SettingsCache settings, ILogging logger)
    {
        _db = db;

        switch (settings.Get(ConfigKeys.MetadataProviderId, 0))
        {
            default:
                _provider = new AniListProvider(logger);
                break;

            case 1:
                _provider = new TenraiMetadataProvider(logger);
                break;
        }

        _linkProperty = typeof(Model_Link).GetProperty(_provider.getLinkPropertyName)!;

        IEntityType model = _db.Model.FindEntityType(typeof(Model_Link))!;

        _linkLinkTableName = $"\"{model.GetSchema() ?? "public"}\".\"{model.GetTableName()}\"";
        _linkMediaIdColumnName = model.FindProperty(nameof(Model_Link.MediaId))!.GetColumnName(StoreObjectIdentifier.Table(model.GetTableName()!, model.GetSchema()))!;
        _linkLinkKeyColumnName = model.FindProperty(_provider.getLinkPropertyName)!.GetColumnName(StoreObjectIdentifier.Table(model.GetTableName()!, model.GetSchema()))!;
    }

    public async Task<Model_Link[]> GetLinksForMedia(IEnumerable<int> mediaIds)
        => await _db.links.Where(l => mediaIds.Contains(l.MediaId)).ToArrayAsync();

    private async Task<List<int>> MapProviderIdsToMediaIdsList(IEnumerable<string> providerIds)
        => (await MapProviderIdsToMediaIdsLookup(providerIds)).Values.ToList();

    private async Task<int[]> MapProviderIdsToMediaIdsArray(IEnumerable<string> providerIds)
        => (await MapProviderIdsToMediaIdsLookup(providerIds)).Values.ToArray();

    private record LinkKeyMediaId(string LinkKey, int MediaId);
    private async Task<Dictionary<string, int>> MapProviderIdsToMediaIdsLookup(IEnumerable<string> providerIds)
    {
        if (providerIds.Count() == 0)
            return new();

        var paramNames = providerIds.Select((_, i) => "@p" + i).ToArray();
        var sql = $"""
            SELECT CAST("{_linkLinkKeyColumnName}" AS TEXT) AS "LinkKey", "{_linkMediaIdColumnName}" AS "MediaId"
            FROM {_linkLinkTableName}
            WHERE CAST("{_linkLinkKeyColumnName}" AS TEXT) IN ({string.Join(",", paramNames)})
            """;

        var parameters = providerIds.Select((id, i) => new NpgsqlParameter(paramNames[i], id)).ToArray();
        LinkKeyMediaId[] rows = await _db.Database
            .SqlQueryRaw<LinkKeyMediaId>(sql, parameters)
            .ToArrayAsync();

        return rows
            .Where(r => r.LinkKey is not null)
            .ToDictionary(r => r.LinkKey!, r => r.MediaId);
    }

    public async Task<Model_MediaEpisode[]> GetEpisodeData(int mediaId)
    {
        Model_Link link = await _db.links.SingleAsync(l => l.MediaId == mediaId);

        var res = await _provider.GetEpisodeData(link);
        Dictionary<string, int> mappings = await MapProviderIdsToMediaIdsLookup(res.Select(r => r.providerId));

        foreach (MediaEpisodeCreationModel ep in res)
            ep.MediaId = mappings[ep.providerId];

        return res;
    }

    public async Task<Model_Media[]> GetMediaInfo(IEnumerable<int> mediaIds)
    {
        var links = await GetLinksForMedia(mediaIds);
        var res = await _provider.GetMediaInfo(links);
        Dictionary<string, int> mappings = await MapProviderIdsToMediaIdsLookup(res.Select(r => r.providerId));

        foreach (var m in res)
            m.Id = mappings[m.providerId];

        return res;
    }

    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria()
        => _provider.GetSearchCriteria();

    public async Task<Dictionary<int, long?>> GetTimeOfNextEpisodes(ICollection<int> ids)
    {
        Model_Link[] links = await GetLinksForMedia(ids);
        Dictionary<string, int> mappings = links.ToDictionary(GetLinkKey, l => l.MediaId);

        var res = await _provider.GetTimeOfNextEpisodes(links);
        return res.ToDictionary(r => mappings[r.Key], r => r.Value);

        string GetLinkKey(Model_Link link) => _linkProperty.GetValue(link)!.ToString()!;
    }

    public async Task<List<int>> GetTrending(int limit)
    {
        var res = await _provider.GetTrending(limit);
        return await MapProviderIdsToMediaIdsList(res);
    }

    public async Task<(int total, int[] providerIds)> SearchMedia(MediaSearchQuery filter)
    {
        var res = await _provider.SearchMedia(filter);
        return (res.total, await MapProviderIdsToMediaIdsArray(res.providerIds));
    }

    public async Task<(int total, int[] providerIds)> SearchSeasonal(MediaSearchQuery filter)
    {
        var res = await _provider.SearchSeasonal(filter);
        return (res.total, await MapProviderIdsToMediaIdsArray(res.providerIds));
    }

    public async Task<Dictionary<int, long>> UpcomingMedia(int? day)
    {
        var res = await _provider.UpcomingMedia(day);
        return await ReconstructMediaLookup(res);
    }

    public async Task<Dictionary<int, int?>> FetchRecommendedMedia(Model_Link media)
    {
        var res = await _provider.FetchRecommendedMedia(media);
        return await ReconstructMediaLookup(res);
    }

    private async Task<Dictionary<int, T>> ReconstructMediaLookup<T>(Dictionary<string, T> originalData)
    {
        Dictionary<string, int> mappings = await MapProviderIdsToMediaIdsLookup(originalData.Keys);
        Dictionary<int, T> reconstructedLookup = new Dictionary<int, T>();

        foreach (var item in originalData)
            if (mappings.TryGetValue(item.Key, out int mediaId))
                reconstructedLookup.Add(mediaId, item.Value);

        return reconstructedLookup;
    }
}
