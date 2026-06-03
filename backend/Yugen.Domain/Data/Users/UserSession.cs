using Yugen.Domain.Models;

namespace Yugen.Domain.Data.Users;

public class UserSession
{
    public required UserModel User { get; set; }
    public required string JellyfinId { get; set; }
    public required string AccessToken { get; set; }

    public static UserSession Master => new UserSession()
    {
        User = new UserModel()
        {
            Id = Guid.Empty,
            ProviderId = "Master",
        },

        AccessToken = Guid.Empty.ToString(),
        JellyfinId = Guid.Empty.ToString()
    };
}
