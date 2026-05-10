using System.Security.Authentication;
using Yugen.Domain.Models;

namespace Yugen.Api.Helpers;

public static class UserAuthHelper
{
    public static void GetUserFromSession(this Microsoft.AspNetCore.Http.HttpContext context, out UserModel user)
    {
        if (context.Items.TryGetValue("User", out object? o) && o != null)
        {
            user = (o as UserModel)!;
            return;
        }

        throw new AuthenticationException("Failed to get user from session");
    }
}
