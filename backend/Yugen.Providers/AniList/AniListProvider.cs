using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        string query = @"
        query Page($idIn: [Int]) {
            Page {
                media(id_in: $idIn) {
                    type
                    tags {
                        name
                        category
                        isAdult
                    }
                    popularity
                    meanScore
                    isAdult
                    idMal
                    id
                    genres
                    format
                    episodes
                    coverImage {
                        extraLarge
                        large
                        medium
                        color
                    }
                    averageScore
                    bannerImage
                    status
                    title {
                        native
                        english
                    }
                }
            }
        }";

        AniListResponse_Search? res = await SendRequest<AniListResponse_Search>(query, new { idIn = aniListIds.ToArray() });

        if (res == null)
            throw new Exception("Failed");

        return res.data.page.media?.Select(x => new Model_Media()
        {
            Id = x.id,
            MalId = x.malId,
            EpisodeCount = x.episodes ?? 0,

            Title = x.title?.getBestMatch ?? "",

            BannerImage = x.bannerImage,
            CardImageLarge = x.coverImage?.extraLarge,
            CardImageSmall = x.coverImage?.medium,
            Colour = x.coverImage?.color


        }).ToArray() ?? [];
    }

    public async Task<MediaCard[]> SearchMedia(string textFilter)
    {
        string query = @"
        query ($search: String!) {
            Page {
                media(search: $search, type: ANIME) {
                id
                title {
                    romaji
                    english
                    native
                }
                }
            }
        }";

        AniListResponse_Search? res = await SendRequest<AniListResponse_Search>(query, new { search = textFilter });
        return [];
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

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
