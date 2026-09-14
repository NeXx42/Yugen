using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Helpers;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Tenrai;

public class TenraiMetadataProvider : IMetaDataProvider
{
    private readonly RestfulHelper _http;
    private readonly SemaphoreSlim _concurrentRequestLimit;

    private const int MAX_TAKE = 50;
    private const int MAX_CONCURRENCY = 4;

    private static ConcurrentDictionary<int, TenraiMetadata_Responses_Anime?> animeDetailsCache = new(); // to save round trips as there is no bulk search by ids

    public TenraiMetadataProvider(ILogging logger)
    {
        _http = new RestfulHelper("https://api.tenrai.org/v1/", logger);
        _concurrentRequestLimit = new SemaphoreSlim(4);
    }

    public string getLinkPropertyName => nameof(Model_Link.mal_id);

    public async Task<MediaCreationModel[]> GetMediaInfo(IEnumerable<Model_Link> items)
    {
        List<MediaCreationModel> results = new List<MediaCreationModel>();
        List<TenraiMetadata_Responses_Anime> metadata = await InternalMediaDetailsSearch(items.Where(i => i.mal_id.HasValue).Select(i => i.mal_id!.Value));

        return metadata.Select(media => new MediaCreationModel()
        {
            Id = -1,
            providerId = media.mal_id.ToString(),

            TitleNative = media.title_japanese ?? media.title,
            TitleEnglish = media.title_english,

            CardImageLarge = media.images?.webp?.large_image_url,
            CardImageSmall = media.images?.webp?.small_image_url,

            BannerImage = media.trailer?.images?.getBestBanner,
            thumbnailIcon = media.images?.webp?.image_url,

            Description = media.synopsis,
            AverageScore = media.score.HasValue ? (int)Math.Round(media.score.Value * 100) : null,
            MeanScore = media.score.HasValue ? (int)Math.Round(media.score.Value * 100) : null,

            StartDate = media.getAiredFrom,
            EndDate = media.getAiredTo,
            NextEpisodeReleaseDate = media.getNextEpisodeDate,

            Status = MapStatus(media.status),
            EpisodeCount = media.episodes,
            Season = media.season.ParseEnumNullable<MediaSeason>(),
            Year = media.year,
            MediaFormat = media.type,

            Genres = new Collection<Model_MediaGenre>(media.genres?.Select(g => new Model_MediaGenre
            {
                Genre = g.name!,
                MediaId = -1
            }).ToList() ?? [])

        }).ToArray();

        MediaStatus? MapStatus(string? name)
        {
            switch (name)
            {
                case "Finished Airing": return MediaStatus.FINISHED;
                case "Currently Airing": return MediaStatus.RELEASING;
                case "Not yet aired": return MediaStatus.NOT_YET_RELEASED;
                default: return null;
            }
        }
    }

    private async Task<List<TenraiMetadata_Responses_Anime>> InternalMediaDetailsSearch(IEnumerable<int> ids)
    {
        ConcurrentBag<TenraiMetadata_Responses_Anime> results = new();
        Stopwatch batchWatcher = new Stopwatch();

        for (int i = 0; i < ids.Count(); i += MAX_CONCURRENCY)
        {
            batchWatcher.Reset();
            batchWatcher.Start();

            await Task.WhenAll(ids.Skip(i).Take(MAX_CONCURRENCY).Select(item => HandleRequest(item)));
            batchWatcher.Stop();

            long ratelimitCooldown = 1_000 - batchWatcher.ElapsedMilliseconds;

            if (ratelimitCooldown > 0)
                await Task.Delay((int)ratelimitCooldown);
        }

        async Task HandleRequest(int id)
        {
            try
            {
                await _concurrentRequestLimit.WaitAsync();
                string uri = Path.Combine("anime", id.ToString(), "full");

                const int maxAttempts = 3;
                for (int i = 0; i < maxAttempts; i++)
                    try
                    {
                        if (!animeDetailsCache.TryGetValue(id, out TenraiMetadata_Responses_Anime? res))
                        {

                            res = (await _http.SendRequest<TenraiMetadata_Responses_Container<TenraiMetadata_Responses_Anime>>(uri, HttpMethod.Get))?.data;
                            animeDetailsCache.AddOrUpdate(id, _ => res, (_, __) => res);
                        }

                        if (res != null)
                            results.Add(res);

                        break;
                    }
                    catch (OverflowException e) // too many request, can try again
                    {
                        if (e.Message.Contains("You have exceeded the 120 requests/minute limit"))
                        {
                            await Task.Delay(60 * 1000);
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                    catch
                    {
                        break;
                    }
            }
            finally
            {
                _concurrentRequestLimit.Release();
            }
        }

        return results.ToList();
    }

    public async Task<MediaEpisodeCreationModel[]> GetEpisodeData(Model_Link media)
    {
        if (!media.mal_id.HasValue)
            return [];

        string uri = $"anime/{media.mal_id}/episodes";
        var res = await _http.SendRequest<TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Episode>>(uri, HttpMethod.Get);

        if ((res?.data?.Length ?? 0) == 0)
            return [];

        return res!.data!.Select(Map).ToArray();

        MediaEpisodeCreationModel Map(TenraiMetadata_Responses_Episode ep) =>
            new MediaEpisodeCreationModel
            {
                EpisodeNumber = ep.mal_id,
                EpisodeTitle = ep.title ?? ep.title_japanese,

                providerId = media.mal_id.ToString()!,
                IsFiller = ep.filler ?? false,
                IsRecap = ep.recap ?? false,
                Score = ep.score.HasValue ? ep.score.Value : null,

                EpisodeIcon = ep.images?.jpeg?.image_url
            };
    }

    // prob need to remove this; left over from anilist setup, but with the move to agnostic providers prob need to just seed the database with set values and map them back instead
    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria() => throw new NotImplementedException();

    public async Task<Dictionary<string, long?>> GetTimeOfNextEpisodes(ICollection<Model_Link> ids)
    {
        List<TenraiMetadata_Responses_Anime> metadata = await InternalMediaDetailsSearch(ids.Where(i => i.mal_id.HasValue).Select(i => i.mal_id!.Value));
        return metadata.ToDictionary(r => r.mal_id.ToString(), r => r.getNextEpisodeDate);
    }

    public async Task<List<string>> GetTrending(int limit)
    {
        List<string> resIds = new List<string>();
        await HandlePageResponse($"top/anime?limit={limit}&sfw=true", item => resIds.Add(item.mal_id.ToString()));

        return resIds;
    }

    public async Task<(int total, string[] providerIds)> SearchMedia(MediaSearchQuery filter)
    {
        List<string> queryParams = new();
        if (!string.IsNullOrEmpty(filter.text)) queryParams.Add($"q={filter.text}");
        queryParams.Add($"page={filter.page}");
        queryParams.Add($"limit={GetTakeSize(filter.pageSize)}");
        queryParams.Add($"sfw={GetSFWFilter(filter.allowAdultContent)}");

        List<string> responseIds = new();
        var resContainer = await HandlePageResponse($"anime?{string.Join("&", queryParams)}", item => responseIds.Add(item.mal_id.ToString()));

        if (resContainer == null)
            return (0, []);

        return (resContainer.pagination!.Value.items.count!.Value, [.. responseIds]);
    }

    public async Task<(int total, string[] providerIds)> SearchSeasonal(MediaSearchQuery filter)
    {
        List<string> queryParams = [
            $"page={filter.page}",
            $"limit={GetTakeSize(filter.pageSize)}",
            $"order_by=start_date",
            $"sort=asc",
            $"sfw={GetSFWFilter(filter.allowAdultContent)}",
        ];

        List<string> responseIds = new();

        string uri = Path.Combine("seasons", filter.year.ToString()!, $"{filter.season!.ToLower()}?{string.Join("&", queryParams)}");
        var resContainer = await HandlePageResponse(uri, item => responseIds.Add(item.mal_id.ToString()));

        if (resContainer == null)
            return (0, []);

        return (resContainer.pagination!.Value.items.count!.Value, [.. responseIds]);
    }

    public async Task<Dictionary<string, long>> UpcomingMedia(int? day)
    {
        DateTime now = DateTime.UtcNow;
        DateTime startDate = new DateTime(now.Year, now.Month, day ?? now.Day, 0, 0, 0, DateTimeKind.Utc);

        TimeZoneInfo japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        DateTimeOffset japanTime = TimeZoneInfo.ConvertTime(startDate, japanTimeZone);

        string dayText = japanTime.DayOfWeek.ToString().ToLower();

        Dictionary<string, long> results = new();
        await HandlePageResponse($"schedules?filter={dayText}&sfw=true", item =>
        {
            long? nextEp = item.getNextEpisodeDate;

            if (!nextEp.HasValue)
                return;

            animeDetailsCache.AddOrUpdate(item.mal_id, _ => item, (_, __) => item);
            results.Add(item.mal_id.ToString(), nextEp ?? 0);
        });

        return results;
    }

    public async Task<Dictionary<string, int?>> FetchRecommendedMedia(Model_Link media)
    {
        if (!media.mal_id.HasValue)
            return [];

        string uri = $"anime/{media.mal_id.Value}/recommendations";
        TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Recommended>? res = await _http.SendRequest<TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Recommended>?>(uri, HttpMethod.Get);

        if ((res?.data?.Length ?? 0) == 0)
            return [];

        return res!.data!
            .OrderByDescending(d => d.votes)
            .Take(10)
            .ToDictionary(d => d.entry.mal_id.ToString(), d => (int?)d.votes);
    }

    private int GetTakeSize(int? desired) => Math.Min(desired ?? 10, MAX_TAKE);
    private string GetSFWFilter(bool? allowAdultContent) => (!(allowAdultContent ?? false)).ToString().ToLower();

    private async Task<TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Anime>?> HandlePageResponse(string uri, Action<TenraiMetadata_Responses_Anime> handler)
    {
        TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Anime>? res = await _http.SendRequest<TenraiMetadata_Responses_Page<TenraiMetadata_Responses_Anime>>(uri, HttpMethod.Get);

        if ((res?.data?.Length ?? 0) == 0)
            return null;

        foreach (var item in res!.data!)
        {
            handler(item);
            animeDetailsCache.AddOrUpdate(item.mal_id, _ => item, (_, __) => item);
        }

        return res;
    }
}
