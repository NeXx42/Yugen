using System.Security.Authentication;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;

namespace Yugen.Api.Helpers;

public static class UserAuthHelper
{
    public static void GetUserFromSession(this Microsoft.AspNetCore.Http.HttpContext context, out UserSession user)
    {
        if (context.Items.TryGetValue("User", out object? o) && o != null)
        {
            user = (o as UserSession)!;
            return;
        }

        throw new AuthenticationException("Failed to get user from session");
    }
}
