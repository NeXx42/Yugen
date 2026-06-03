using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;

namespace Yugen.Providers;

public interface INotificationService
{
    public Task<Model_Notification[]> Consume(string json, Func<int?, Task<Model_Link?>> lookupExternalId, Func<int, Task> refreshDownloads);
}
