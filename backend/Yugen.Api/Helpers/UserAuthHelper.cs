using System.Security.Authentication;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;

namespace Yugen.Api.Helpers;

public static class UserAuthHelper
{
    public static void GetUserFromSession(this Microsoft.AspNetCore.Http.HttpContext context, out UserSession user)
    {
        context.TryGetUserFromSession(out UserSession? usr);

        if (usr != null)
        {
            user = usr;
            return;
        }

        throw new AuthenticationException("Failed to get user from session");
    }

    public static void TryGetUserFromSession(this Microsoft.AspNetCore.Http.HttpContext context, out UserSession? user)
    {
        if (context.Items.TryGetValue("User", out object? o) && o != null)
        {
            user = (o as UserSession)!;
            return;
        }

        user = null;
    }
}
