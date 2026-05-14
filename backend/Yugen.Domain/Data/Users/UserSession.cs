using Yugen.Domain.Models;

namespace Yugen.Domain.Data.Users;

public class UserSession
{
    public required UserModel User { get; set; }
    public required string JellyfinId { get; set; }
    public required string AccessToken { get; set; }
}
