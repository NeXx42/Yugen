using System.Linq.Expressions;
using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers;

public interface IMetaDataProvider
{
    public string getLinkPropertyName { get; }

    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria();

    public Task<(int total, string[] providerIds)> SearchMedia(MediaSearchQuery filter);
    public Task<(int total, string[] providerIds)> SearchSeasonal(MediaSearchQuery filter);

    public Task<List<string>> GetTrending(int limit);
    public Task<Dictionary<string, long>> UpcomingMedia(int? day);

    public Task<MediaCreationModel[]> GetMediaInfo(IEnumerable<Model_Link> items);
    public Task<MediaEpisodeCreationModel[]> GetEpisodeData(Model_Link media);
    public Task<string[]> FetchRecommendedMedia(Model_Link media);

    public Task<Dictionary<string, long?>> GetTimeOfNextEpisodes(ICollection<Model_Link> ids);
}
