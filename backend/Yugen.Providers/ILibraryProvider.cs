using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;

namespace Yugen.Providers;

public interface ILibraryProvider
{
    public Task<Model_DownloadedEpisode[]?> GetDownloadedEpisodes(int mediaId, Model_Link link);
}
