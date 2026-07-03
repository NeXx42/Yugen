using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;
using Yugen.Providers.Jikan;

namespace Yugen.Providers.AniList;

public class AniListProvider : IMetaDataProvider
{
    private readonly string _url;
    private readonly HttpClient _http;

    private readonly IMetaDataProvider _episodeProvider;

    public AniListProvider()
    {
        _url = "https://graphql.anilist.co";
        _http = new HttpClient();

        _episodeProvider = new JikanMetadataProvider();
    }

    public async Task<Model_Media[]> GetMediaInfo(MediaSearchQuery filter)
    {
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
                await Task.Delay(200); // dont want to spam their servers
            }
            catch (Exception e)
            {

            }
        }


        List<Model_Media> results = new List<Model_Media>();

        foreach (AniListResponse_Media media in responses)
        {
            Model_Media result = new Model_Media()
            {
                Id = media.id,

                TitleEnglish = media.title?.english,
                TitleNative = media.title?.native,
                Description = media.description,
                Status = media.status,
                MediaFormat = media.format,
                SiteUrl = media.siteUrl,

                Duration = media.duration,
                EpisodeCount = media.episodes,
                Season = media.season,
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

            for (int i = 0; i < (media.streamingEpisodes?.Length ?? 0); i++)
            {
                result.Episodes.Add(new Model_MediaEpisode()
                {
                    MediaId = media.id,
                    EpisodeNumber = i + 1,

                    EpisodeTitle = Regex.Replace(media.streamingEpisodes![i].title ?? "", @"^Episode \d+ - ", ""),
                    EpisodeIcon = media.streamingEpisodes![i].thumbnail,
                });
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

    public async Task<(int, int[])> SearchMedia(MediaSearchQuery searchQuery)
    {
        AniListResponse_Search? res = await GenerateGraphqlQuery(@"
            id
        ", searchQuery);

        if (res == null)
            return (0, []);

        return (res.data.page?.pageInfo?.total ?? 0, res.data.page?.media?.Select(m => m.id).ToArray() ?? []);
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
            idIn = searchQuery?.ids,

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

    public async Task<Dictionary<int, long>> UpcomingMedia()
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
            airingAtGreater = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            airingAtLesser = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),
            sort = "TIME"
        });

        if (res == null)
            throw new Exception("Failed");

        Dictionary<int, long> results = new Dictionary<int, long>();

        foreach (var entry in res.data.page.airingSchedules!)
        {
            if (results.TryGetValue(entry.mediaId, out long nextEpisode) && nextEpisode < entry.airingAt)
                continue;

            results[entry.mediaId] = entry.airingAt;
        }

        return results;
    }

    public async Task<Dictionary<int, long?>> GetTimeOfNextEpisodes(ICollection<int> aniListIds)
    {
        if (aniListIds.Count == 0)
            return new Dictionary<int, long?>();

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

        Dictionary<int, long?> response = new Dictionary<int, long?>();

        foreach (int id in aniListIds)
            response[id] = null;

        AniListResponse_AiringEpisode? res = await SendRequest<AniListResponse_AiringEpisode>(query, new { idIn = response.Keys, perPage = response.Count });

        if ((res?.data?.page?.media?.Length ?? 0) > 0)
        {
            foreach (AniListResponse_AiringEpisode.Data.Page.Media? entry in res!.data!.page!.media!)
            {
                if (entry == null)
                    continue;

                response[entry.id] = entry?.nextAiringEpisode?.airingAt;
            }
        }

        return response;
    }

    public async Task<List<int>> GetTrending(int limit)
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

        return res.data.trending.media.Select(m => m.id).ToList();
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

    // not possible with AniList
    public Task<Model_MediaEpisode[]> GetEpisodeData(int malId) => _episodeProvider.GetEpisodeData(malId);







    public async Task<T?> SendRequest<T>(string query, object variables)
    {
        var json = JsonSerializer.Serialize(new { query, variables });
        var response = await _http.PostAsync(_url, new StringContent(json, Encoding.UTF8, "application/json"));

        try
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            return default;
        }
    }
}
