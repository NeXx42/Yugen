using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Helpers;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers.AniList;

public class AniListProvider : IMetaDataProvider
{
    private readonly string _url;
    private readonly HttpClient _http;
    private readonly ILogging _logger;


    public AniListProvider(ILogging logger)
    {
        _logger = logger;

        _url = "https://graphql.anilist.co";
        _http = new HttpClient();
    }

    public string getLinkPropertyName => nameof(Model_Link.anilist_id);

    public async Task<MediaCreationModel[]> GetMediaInfo(IEnumerable<Model_Link> items)
    {
        MediaSearchQuery filter = new MediaSearchQuery()
        {
            ids = items.ToArray()
        };

        if ((filter.ids?.Count ?? 0) == 0)
            return [];

        filter.pageSize ??= 50;
        List<AniListResponse_Media> responses = new List<AniListResponse_Media>();

        for (int i = 0; i < filter.ids!.Count; i += filter.pageSize.Value)
        {
            filter.page = i + 1;

            try
            {
                AniListResponse_Search? res = await GenerateGraphqlQuery($@"
                    id
                    idMal
                    title {{
                        romaji
                        english
                        native
                        userPreferred
                    }}
                    type
                    format
                    status
                    description
                    startDate {{
                        year
                        month
                        day
                    }}
                    endDate {{
                        year
                        month
                        day
                    }}
                    season
                    seasonYear
                    episodes
                    duration
                    chapters
                    volumes
                    countryOfOrigin
                    isLicensed
                    source
                    hashtag
                    trailer {{
                        id
                        site
                        thumbnail
                    }}
                    updatedAt
                    coverImage {{
                        extraLarge
                        large
                        medium
                        color
                    }}
                    bannerImage
                    genres
                    synonyms
                    averageScore
                    meanScore
                    popularity
                    isLocked
                    trending
                    favourites
                    tags {{
                        id
                    }}
                    isFavourite
                    isFavouriteBlocked
                    isAdult
                    externalLinks {{
                        id
                        url
                        site
                        siteId
                        type
                        language
                        color
                        icon
                        notes
                        isDisabled
                    }}
                    streamingEpisodes {{
                        title
                        thumbnail
                        url
                        site
                    }}
                    rankings {{
                        id
                        rank
                        type
                        format
                        year
                        season
                        allTime
                        context
                    }}
                    recommendations {{
                        nodes {{
                            mediaRecommendation{{
                                id
                            }}
                        }}
                    }}
                    siteUrl
                    autoCreateForumThread
                    isRecommendationBlocked
                    isReviewBlocked
                    modNotes
                    nextAiringEpisode {{
                        airingAt
                    }}
                ", filter);

                if (res?.data?.page?.media == null)
                    throw new Exception("Failed");

                responses.AddRange(res.data.page.media);
                await Task.Delay(200); // don't want to spam their servers
            }
            catch (Exception e)
            {
                _logger.LogError(e);
            }
        }


        List<MediaCreationModel> results = new List<MediaCreationModel>();

        foreach (AniListResponse_Media media in responses)
        {
            MediaCreationModel result = new MediaCreationModel()
            {
                Id = -1,
                providerId = media.id.ToString(),

                TitleEnglish = media.title?.english,
                TitleNative = media.title?.native,
                Description = media.description,
                Status = media.status.ParseEnumNullable<MediaStatus>(),
                MediaFormat = media.format,
                SiteUrl = media.siteUrl,

                Duration = media.duration,
                EpisodeCount = media.episodes,
                Season = media.season.ParseEnumNullable<MediaSeason>(),
                Year = media.seasonYear,
                AverageScore = media.averageScore,
                MeanScore = media.meanScore,
                StartDate = media.startDate?.ToUnix(),
                EndDate = media.endDate?.ToUnix(),

                BannerImage = media.bannerImage,
                CardImageLarge = media.coverImage?.extraLarge,
                CardImageSmall = media.coverImage?.medium,
                Colour = media.coverImage?.color,
                thumbnailIcon = media.trailer?.thumbnail,

                NextEpisodeReleaseDate = media.nextAiringEpisode?.airingAt
            };

            for (int i = 0; i < (media.tags?.Length ?? 0); i++)
            {
                result.Tags.Add(new Model_MediaTag()
                {
                    MediaId = media.id,
                    TagId = media.tags![i].id
                });
            }

            for (int i = 0; i < (media.genres?.Length ?? 0); i++)
            {
                result.Genres.Add(new Model_MediaGenre()
                {
                    MediaId = media.id,
                    Genre = media.genres![i]
                });
            }

            if ((media.streamingEpisodes?.Length ?? 0) > 0)
            {
                for (int i = 0; i < (media.streamingEpisodes?.Length ?? 0); i++)
                {
                    result.Episodes.Add(new MediaEpisodeCreationModel()
                    {
                        MediaId = -1,
                        providerId = media.id.ToString(),

                        EpisodeNumber = i + 1,

                        EpisodeTitle = Regex.Replace(media.streamingEpisodes![i].title ?? "", @"^Episode \d+ - ", ""),
                        EpisodeIcon = media.streamingEpisodes![i].thumbnail,
                    });
                }
            }
            else
            {
                MediaEpisodeCreationModel[] eps = FakeEpisodeList(media.id.ToString(), result.EpisodeCount);

                foreach (MediaEpisodeCreationModel ep in eps)
                    result.Episodes.Add(ep);
            }

            for (int i = 0; i < (media.recommendations?.nodes?.Length ?? 0); i++)
            {
                if (media.recommendations?.nodes?[i]?.mediaRecommendation?.id == null)
                    continue;

                result.RelatedMedia.Add(new Model_MediaRelation()
                {
                    MediaId = media.id,
                    ConnectedMediaId = media.recommendations!.nodes![i].mediaRecommendation.id
                });
            }

            results.Add(result);
        }

        return results.ToArray();
    }

    public async Task<(int, string[])> SearchSeasonal(MediaSearchQuery searchQuery)
        => await SearchMedia(searchQuery);

    public async Task<(int, string[])> SearchMedia(MediaSearchQuery searchQuery)
    {
        AniListResponse_Search? res = await GenerateGraphqlQuery(@"
            id
        ", searchQuery);

        if (res == null)
            return (0, []);

        return (res.data.page?.pageInfo?.total ?? 0, res.data.page?.media?.Select(m => m.id.ToString()).ToArray() ?? []);
    }

    private async Task<AniListResponse_Search?> GenerateGraphqlQuery(string fields, MediaSearchQuery? searchQuery)
    {
        List<string> inputs = new List<string>();
        List<string> vars = new List<string>()
        {
            "type: ANIME"
        };

        TryAddQueryFilter_String(searchQuery?.text, "search", "search", "String!");
        TryAddQueryFilter_Generic(searchQuery?.sort, "sort", "sort", "[MediaSort]");
        TryAddQueryFilter_String(searchQuery?.season, "season", "season", "MediaSeason");
        TryAddQueryFilter_String(searchQuery?.format, "format", "format", "MediaFormat");

        TryAddQueryFilter_Generic(searchQuery?.year, "seasonYear", "seasonYear", "Int");
        TryAddQueryFilter_Generic(searchQuery?.allowAdultContent, "isAdult", "isAdult", "Boolean");
        TryAddQueryFilter_Generic(searchQuery?.lesserStartDate, "startDate_lesser", "startDateLesser", "FuzzyDateInt");

        TryAddQueryFilter_Collection(searchQuery?.ids, "id_in", "idIn", "[Int]");
        TryAddQueryFilter_Collection(searchQuery?.tags, "tag_in", "tagIn", "[String]");
        TryAddQueryFilter_Collection(searchQuery?.genres, "genre_in", "genreIn", "[String]");

        return await SendRequest<AniListResponse_Search>(@$"query Page{(inputs.Count > 0 ? $"({string.Join(",", inputs)})" : "")} {{
            Page(perPage: {searchQuery?.pageSize ?? 10}, page: {searchQuery?.page ?? 1}) {{
                media ({string.Join(",", vars)}) {{
                    {fields}
                }}
                pageInfo {{
                    total
                }}
            }}
        }}", new
        {
            isAdult = false,
            sort = searchQuery?.sort?.ToString() ?? "",

            search = searchQuery?.text,
            idIn = searchQuery?.ids?.Where(i => i.anilist_id.HasValue).Select(i => i.anilist_id!.Value),

            startDate_lesser = searchQuery?.lesserStartDate,
            seasonYear = searchQuery?.year,
            season = searchQuery?.season,
            status = searchQuery?.status,
            format = searchQuery?.format,
            genreIn = searchQuery?.genres,
            tagIn = searchQuery?.tags
        });

        void TryAddQueryFilter_String(string? val, string propertyName, string varName, string type)
        {
            if (!string.IsNullOrEmpty(val)) AddQueryFilter(propertyName, varName, type);
        }

        void TryAddQueryFilter_Generic<T>(T? val, string propertyName, string varName, string type)
        {
            if (val != null) AddQueryFilter(propertyName, varName, type);
        }

        void TryAddQueryFilter_Collection<T>(ICollection<T>? val, string propertyName, string varName, string type)
        {
            if ((val?.Count ?? 0) > 0) AddQueryFilter(propertyName, varName, type);
        }

        void AddQueryFilter(string propertyName, string varName, string type)
        {
            inputs.Add($"${varName}: {type}");
            vars.Add($"{propertyName}: ${varName}");
        }
    }

    public async Task<Dictionary<string, long>> UpcomingMedia(int? day)
    {
        if (day.HasValue)
        {
            DateTime now = DateTime.UtcNow;
            DateTime startDate = new DateTime(now.Year, now.Month, day.Value, 0, 0, 0, DateTimeKind.Utc);

            if (day < now.Day)
            {
                startDate = startDate.AddMonths(1);
            }

            DateTimeOffset start = new DateTimeOffset(startDate);
            DateTimeOffset end = start.AddHours(12);

            return await SearchForMediaBetweenTime(start, end);
        }

        return await SearchForMediaBetweenTime(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));
    }

    private async Task<Dictionary<string, long>> SearchForMediaBetweenTime(DateTimeOffset start, DateTimeOffset end)
    {
        string query = @"
        query Page($airingAtGreater: Int, $airingAtLesser: Int, $sort: [AiringSort]) {
            Page {
                airingSchedules(airingAt_greater: $airingAtGreater, airingAt_lesser: $airingAtLesser, sort: $sort) {
                    mediaId
                    timeUntilAiring
                    airingAt
                }
            }
        }";

        AniListResponse_Airing? res = await SendRequest<AniListResponse_Airing>(query, new
        {
            airingAtGreater = start.ToUnixTimeSeconds(),
            airingAtLesser = end.ToUnixTimeSeconds(),
            sort = "TIME"
        });

        if (res == null)
            throw new Exception("Failed");

        Dictionary<string, long> results = new Dictionary<string, long>();

        foreach (var entry in res.data.page.airingSchedules!)
        {
            if (results.TryGetValue(entry.mediaId.ToString(), out long nextEpisode) && nextEpisode < entry.airingAt)
                continue;

            results[entry.mediaId.ToString()] = entry.airingAt;
        }

        return results;
    }

    public async Task<Dictionary<string, long?>> GetTimeOfNextEpisodes(ICollection<Model_Link> aniListIds)
    {
        if (aniListIds.Count == 0)
            return new Dictionary<string, long?>();

        string query = @"query Page($perPage: Int, $idIn: [Int]) {
            Page(perPage: $perPage) {
                media(id_in: $idIn) {
                    id
                    nextAiringEpisode {
                        airingAt
                    }
                }
            }
        }";

        Dictionary<string, long?> response = new Dictionary<string, long?>();

        foreach (Model_Link id in aniListIds)
            if (id.anilist_id.HasValue)
                response[id.anilist_id.Value.ToString()] = null;

        AniListResponse_AiringEpisode? res = await SendRequest<AniListResponse_AiringEpisode>(query, new
        {
            idIn = aniListIds.Where(a => a.anilist_id.HasValue).Select(a => a.anilist_id!.Value),
            perPage = response.Count
        });

        if ((res?.data?.page?.media?.Length ?? 0) > 0)
        {
            foreach (AniListResponse_AiringEpisode.Data.Page.Media? entry in res!.data!.page!.media!)
            {
                if (entry == null)
                    continue;

                response[entry.id.ToString()] = entry?.nextAiringEpisode?.airingAt;
            }
        }

        return response;
    }

    public async Task<List<string>> GetTrending(int limit)
    {
        string query = @$"query {{
            trending: Page(page: 1, perPage: {limit}) {{
                media(sort: TRENDING_DESC, type: ANIME, isAdult: false) {{
                    id
                }}
            }}
        }}";

        AniListResponse_Trending? res = await SendRequest<AniListResponse_Trending>(query, new { });

        if (res == null)
            return [];

        return res.data.trending.media.Select(m => m.id.ToString()).ToList();
    }


    public async Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria()
    {
        string query = @"query Query {
            GenreCollection
            MediaTagCollection {
                name
                id
                isAdult
                isMediaSpoiler
                category
                rank
                description
            }
        }";

        AniListResponse_Criteria? res = await SendRequest<AniListResponse_Criteria>(query, new { });

        if (res == null)
            return ([], []);

        return (
            res.data.mediaTagCollection.Select(x => new Model_Tag()
            {
                Id = x.id,

                IsAdult = x.isAdult,
                IsMediaSpoiler = x.isMediaSpoiler,
                IsGeneralSpoiler = x.isGeneralSpoiler,

                Name = x.name,
                Category = x.category,
                Description = x.description

            }).ToList(),

            res.data.genreCollection.Select(x => new Model_Genre()
            {
                Genre = x
            }).ToList()
        );
    }


    public async Task<MediaEpisodeCreationModel[]> GetEpisodeData(Model_Link media)
    {
        if (!media.anilist_id.HasValue)
            return [];

        string query = @"query Query($mediaId: Int) {
            Media(id: $mediaId) {
                episodes
            }
        }";

        AniListResponse_Info? res = await SendRequest<AniListResponse_Info>(query, new { mediaId = media.anilist_id.Value });

        if ((res?.data?.media?.episodes ?? 0) == 0)
            return [];

        return FakeEpisodeList(media.anilist_id.Value.ToString(), res?.data?.media?.episodes);
    }

    private MediaEpisodeCreationModel[] FakeEpisodeList(string mediaId, int? count)
    {
        if ((count ?? 0) == 0)
            return [];

        return Enumerable.Range(0, count!.Value).Select(e => new MediaEpisodeCreationModel()
        {
            MediaId = -1,
            providerId = mediaId,

            EpisodeNumber = e + 1,
            EpisodeTitle = $"Episode {e + 1}"
        }).ToArray();
    }






    public async Task<T?> SendRequest<T>(string query, object variables)
    {
        var json = JsonSerializer.Serialize(new { query, variables });

        var response = await _http.PostAsync(_url, new StringContent(json, Encoding.UTF8, "application/json"));
        string responseJson = await response.Content.ReadAsStringAsync();

        try
        {
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception e)
        {
            _logger.LogError(new Exception($"{e.Message}\n\n{responseJson}"));
            return default;
        }
    }

    public async Task<Dictionary<string, int?>> FetchRecommendedMedia(Model_Link media)
    {
        if (!media.anilist_id.HasValue)
            return [];

        string query = @"query Query($mediaId: Int) {
            Media(id: $mediaId) {
                recommendations {{
                    nodes {{
                        mediaRecommendation{{
                            id
                        }}
                    }}
                }}
            }
        }";

        AniListResponse_Info? res = await SendRequest<AniListResponse_Info>(query, new { mediaId = media.anilist_id.Value });

        if ((res?.data?.media?.recommendations?.nodes?.Length ?? 0) == 0)
            return [];

        return res!.data.media.recommendations!.nodes!
            .ToDictionary(n => n.mediaRecommendation.id.ToString(), _ => (int?)null);
    }
}
