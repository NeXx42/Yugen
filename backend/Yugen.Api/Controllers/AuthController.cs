using Microsoft.AspNetCore.Mvc;
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
    public async Task<UserSession?> Login([FromBody] LoginRequest req)
    {
        UserSession? session = await _userService.LoginUser(req.username, req.password);

        if (session != null)
        {
            HttpContext.Response.Cookies.Append("AuthToken", session.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        return session;
    }
}
