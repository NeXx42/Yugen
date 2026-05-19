using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;

namespace Yugen.Providers;

public interface ILibraryProvider
{
    public Task<List<int>?> GetDownloadedMedia();
    public Task<Model_DownloadedEpisode[]?> GetDownloadedEpisodes(int mediaId, Model_Link link);

    public Task RequestSeries(int tvdbId, int[] seasons, string rootFolder, int quality);
}
