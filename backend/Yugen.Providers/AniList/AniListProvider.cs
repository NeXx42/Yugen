using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Yugen.Core.Data;
using Yugen.Domain.Enums;
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

    public async Task<Model_Media[]> GetMediaInfo(ICollection<int> aniListIds)
    {
        if (aniListIds.Count == 0)
            return [];

        string query = @"query Media($page: Int, $perPage: Int, $idIn: [Int]) {
            Page(page: $page, perPage: $perPage) {
                media(id_in: $idIn) {
                    id
                    idMal
                    title {
                        romaji
                        english
                        native
                        userPreferred
                    }
                    type
                    format
                    status
                    description
                    startDate {
                        year
                        month
                        day
                    }
                    endDate {
                        year
                        month
                        day
                    }
                    season
                    seasonYear
                    seasonInt
                    episodes
                    duration
                    chapters
                    volumes
                    countryOfOrigin
                    isLicensed
                    source
                    hashtag
                    trailer {
                        id
                        site
                        thumbnail
                    }
                    updatedAt
                    coverImage {
                        extraLarge
                        large
                        medium
                        color
                    }
                    bannerImage
                    genres
                    synonyms
                    averageScore
                    meanScore
                    popularity
                    isLocked
                    trending
                    favourites
                    tags {
                        id
                        name
                        description
                        category
                        rank
                        isGeneralSpoiler
                        isMediaSpoiler
                        isAdult
                        userId
                    }
                    isFavourite
                    isFavouriteBlocked
                    isAdult
                    externalLinks {
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
                    }
                    streamingEpisodes {
                        title
                        thumbnail
                        url
                        site
                    }
                    rankings {
                        id
                        rank
                        type
                        format
                        year
                        season
                        allTime
                        context
                    }
                    recommendations {
                        nodes {
                            id
                        }
                    }
                    siteUrl
                    autoCreateForumThread
                    isRecommendationBlocked
                    isReviewBlocked
                    modNotes
                }
            }
        }";

        AniListResponse_Search? res = await SendRequest<AniListResponse_Search>(query, new { idIn = aniListIds.ToArray(), perPage = aniListIds.Count });

        if (res?.data?.page?.media == null)
            throw new Exception("Failed");

        List<Model_Media> results = new List<Model_Media>();

        foreach (AniListResponse_Media media in res.data.page.media)
        {
            Model_Media result = new Model_Media()
            {
                Id = media.id,
                EpisodeCount = media.episodes ?? 0,

                Title = media.title?.getBestMatch ?? "",
                Description = media.description,
                Status = media.status,
                MediaFormat = media.format,
                SiteUrl = media.siteUrl,

                AverageScore = media.averageScore,
                MeanScore = media.meanScore,

                BannerImage = media.bannerImage,
                CardImageLarge = media.coverImage?.extraLarge,
                CardImageSmall = media.coverImage?.medium,
                Colour = media.coverImage?.color,
                thumbnailIcon = media.trailer?.thumbnail,
            };


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

            results.Add(result);
        }

        return results.ToArray();
    }

    public async Task<(int, int[])> SearchMedia(string textFilter, int page, int pageSize, bool allowAdult)
    {
        string inputs = "";
        string vars = "type: $type";

        if (!string.IsNullOrEmpty(textFilter))
        {
            inputs += ", $search: String!";
            vars += ", search: $search";
        }

        if (!allowAdult)
        {
            inputs += ", $isAdult: Boolean";
            vars += ", isAdult: $isAdult";
        }

        string query = @$"query Page($perPage: Int, $page: Int, $type: MediaType{inputs}) {{
            Page(perPage: $perPage, page: $page) {{
                media({vars}) {{
                    id
                }}
                pageInfo {{
                    total
                }}
            }}
        }}";

        AniListResponse_Search? res = await SendRequest<AniListResponse_Search>(query, new
        {
            search = textFilter,
            perPage = pageSize,
            page = page,
            type = "ANIME",
            isAdult = false,
        });

        if (res == null)
            return (0, []);

        return (res.data.page?.pageInfo?.total ?? 0, res.data.page?.media?.Select(m => m.id).ToArray() ?? []);
    }

    public async Task<Dictionary<int, long>> UpcomingMedia()
    {
        string query = @"
        query Page($airingAtGreater: Int, $airingAtLesser: Int) {
            Page {
                airingSchedules(airingAt_greater: $airingAtGreater, airingAt_lesser: $airingAtLesser) {
                    mediaId
                    timeUntilAiring
                }
            }
        }";

        AniListResponse_Airing? res = await SendRequest<AniListResponse_Airing>(query, new
        {
            airingAtGreater = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            airingAtLesser = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds()
        });

        if (res == null)
            throw new Exception("Failed");

        Dictionary<int, long> results = new Dictionary<int, long>();

        foreach (var entry in res.data.page.airingSchedules!)
        {
            if (results.TryGetValue(entry.mediaId, out long nextEpisode) && nextEpisode < entry.timeUntilAiring)
                continue;

            results[entry.mediaId] = entry.timeUntilAiring;
        }

        return results;
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

    public async Task<long?> GetTimeOfNextEpisode(int id)
    {
        string query = @"query Media($mediaId: Int) {
            Media(id: $mediaId) {
                nextAiringEpisode {
                airingAt
                }
            }
        }";

        AniListResponse_AiringEpisode? res = await SendRequest<AniListResponse_AiringEpisode>(query, new { mediaId = id });
        return res?.data?.media?.nextAiringEpisode?.airingAt;
    }
}
