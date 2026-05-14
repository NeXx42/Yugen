using Yugen.Domain.Data.Users;

namespace Yugen.Providers;

public interface IUserProvider
{
    public Task<ExternalUser[]> GetAllUsers();
    public Task<string?> LoginUser(string username, string password);
}
