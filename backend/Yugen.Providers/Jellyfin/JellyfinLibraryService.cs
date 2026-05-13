// using Yugen.Domain.Data.Media;
// using Yugen.Providers.Helpers;

// namespace Yugen.Providers.Jellyfin;

// public class JellyfinLibraryService : ILibraryProvider
// {
//     private readonly RestfulHelper _http;

//     public JellyfinLibraryService(string url, string apiKey)
//     {
//         _http = new RestfulHelper(url, new Dictionary<string, string>()
//         {
//             { "X-Emby-Token", apiKey}
//         });
//     }

//     public async Task<ExternalMedia[]> GetExternalMedia(string jellyfinUserId)
//     {
//         const string filters = "?Recursive=true&Fields=ProviderIds";
//         JellyfinResponse_Page<JellyfinResponse_Media>? res = await _http.SendRequest<JellyfinResponse_Page<JellyfinResponse_Media>>($"Users/{jellyfinUserId}/Items{filters}");


//         if (res == null)
//             return [];

//         JellyfinResponse_Media[] episodes = res.Items.Where(x => x.type.Equals("Episode")).ToArray();
//         JellyfinResponse_Media[] seasons = res.Items.Where(x => x.type.Equals("Season")).ToArray();
//         JellyfinResponse_Media[] series = res.Items.Where(x => x.type.Equals("Series")).ToArray();

//         var groupedData = series.Select(x =>
//             (
//                 x,
//                 seasons.Where(s => s.seriesId.Equals(x.id))
//                     .Select(s => (s, episodes.Where(e => e.seasonId.Equals(s.id)).ToArray()
//                 )).ToArray()
//             )
//         );

//         return groupedData.Select(x => new ExternalMedia()
//         {
//             jellyfinId = Guid.Parse(x.x.id),
//             title = x.x.name,

//             seasons = x.Item2?.Select(s => new ExternalMedia.Season()
//             {
//                 jellyfinId = Guid.Parse(s.s.id),
//                 title = s.s.name,
//                 number = s.s.indexNumber ?? -1,
//                 aniListId = s.s.providerIds!.AniList,

//                 episodes = s.Item2.Select(e => new ExternalMedia.Season.Episode()
//                 {
//                     jellyfinId = Guid.Parse(e.id),
//                     title = e.name,
//                     number = e.indexNumber ?? -1,

//                 }).ToArray()

//             }).ToArray()

//         }).ToArray();
//     }
// }
