using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Services;
using Yugen.Domain.Data.Users;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("all")]
    public async Task<ExternalUser[]> GetUsers()
    {
        return await _userService.GetAllUsers();
    }

    public struct LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    [HttpPost("login")]
    public async Task<bool> Login([FromBody] LoginRequest req)
    {
        string? accessToken = await _userService.LoginUser(req.username, req.password);

        if (!string.IsNullOrEmpty(accessToken))
        {
            HttpContext.Response.Cookies.Append("AuthToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return true;
        }

        return false;
    }

    [HttpGet("Session")]
    public async Task<UserSession?> GetSession()
    {
        User.TryGetUserFromSession(out JWTUser? usr);
        return await _userService.GetSession(usr);
    }

    [Authorize]
    [HttpPost("SaveSetting")]
    public async Task<IResult> UpdateUserSetting([FromQuery] string key, [FromBody] string? value)
    {
        try
        {
            User.GetUserFromSession(out JWTUser usr);
            await _userService.SaveUserSetting(usr.Id, key, value);

            return Results.Ok();
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    [HttpPost("logout")]
    public async Task Logout()
    {
        HttpContext.Response.Cookies.Delete("AuthToken");
    }
}
