using System.Security.Authentication;
using System.Security.Claims;
using Yugen.Core.Services;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;

namespace Yugen.Api.Helpers;

public static class UserAuthHelper
{
    public static void GetUserFromSession(this ClaimsPrincipal context, out JWTUser user)
    {
        context.TryGetUserFromSession(out JWTUser? usr);

        if (usr != null)
        {
            user = usr;
            return;
        }

        throw new AuthenticationException("Failed to get user from session");
    }

    public static void TryGetUserFromSession(this ClaimsPrincipal context, out JWTUser? user)
    {
        user = null;
        string? userGuid = context.FindFirst(UserService.CLAIM_ID)?.Value;

        if (string.IsNullOrEmpty(userGuid))
            return;

        string? claimJellyfinId = context.FindFirst(UserService.CLAIM_JELLYFIN_ID)?.Value;

        if (Guid.TryParse(userGuid, out Guid id) && !string.IsNullOrEmpty(claimJellyfinId))
        {
            user = new JWTUser()
            {
                Id = id,
                JellyfinId = claimJellyfinId,
            };
        }
    }
}
