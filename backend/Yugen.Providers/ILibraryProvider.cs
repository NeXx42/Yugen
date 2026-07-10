using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;

namespace Yugen.Providers;

public interface ILibraryProvider
{
    public bool isSetup { get; }

    public Task<(string, List<int>)?> GetDownloadedMedia();
    public Task<Model_DownloadedMedia?> GetDownloadedEpisodes(int mediaId, Model_Link link);

    public Task<DownloadRequestInfo> GetRequestInfo(Model_Link link);

    public Task DeleteMedia(Model_DownloadedMedia existingDownload);
    public Task<bool> ResearchMedia(Model_DownloadedMedia existingDownload);
    public Task<Model_DownloadedMedia?> RequestSeries(int mediaId, Model_DownloadedMedia? existingDownload, DownloadRequest request);

    public void EmbedLink(IModel_Link link, int? linkId, int? seasonId);
}
