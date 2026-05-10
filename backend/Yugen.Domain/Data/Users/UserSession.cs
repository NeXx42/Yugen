using Yugen.Domain.Models;

namespace Yugen.Domain.Data.Users;

public class UserSession
{
    public UserModel User { get; set; }
    public object ProviderSession { get; set; }
    public string AccessToken { get; set; }
}
