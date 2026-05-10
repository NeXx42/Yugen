using Yugen.Domain.Data.Users;

namespace Yugen.Providers;

public interface IUserProvider
{
    public Task<ExternalUser[]> GetAllUsers();
    public Task<(object providerSession, ExternalUser externalUserId)?> LoginUser(string username, string password);
}
