using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class UserService
{
    private readonly YugenContext _db;

    private readonly CacheService _cache;
    private readonly IUserProvider _userProvider;

    private readonly byte[] _jwtToken;

    public UserService(YugenContext db, IOptions<EncryptionConfig> encryptionSettings, IOptions<ProviderConfig> providerSettings, CacheService cache)
    {
        _db = db;

        _cache = cache;

        _userProvider = new JellyfinUserService(providerSettings.Value.jellyfin_Url!, providerSettings.Value.jellyfin_ApiKey!);
        _jwtToken = Convert.FromBase64String(encryptionSettings.Value.jwtToken);
    }

    public async Task<UserSession> GetUser(Guid userId)
    {
        string CACHE_KEY = $"USER_SESSION_{userId}";

        if (_cache.TryGetValue(CACHE_KEY, out UserSession? usr))
            return usr!;

        UserModel dbUser = await _db.user.SingleAsync(x => x.Id == userId);
        usr = new UserSession()
        {
            User = dbUser!,
            JellyfinId = dbUser.ProviderId,
            AccessToken = GenerateToken(dbUser)
        };

        _cache.SetIfNotExists(CACHE_KEY, usr);
        return usr;
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        return await _userProvider.GetAllUsers();
    }

    public async Task<UserSession?> LoginUser(string username, string password)
    {
        string? jellyfinId = await _userProvider.LoginUser(username, password);

        if (string.IsNullOrEmpty(jellyfinId))
            return null;

        UserModel? dbUser = _db.user.FirstOrDefault(x => x.ProviderId == jellyfinId);

        if (dbUser == null)
        {
            await _db.AddAsync(new UserModel()
            {
                ProviderId = jellyfinId
            });

            await _db.SaveChangesAsync();
            dbUser = _db.user.FirstOrDefault(x => x.ProviderId == jellyfinId);
        }

        return new UserSession()
        {
            User = dbUser!,
            JellyfinId = dbUser!.ProviderId,
            AccessToken = GenerateToken(dbUser)
        };
    }

    public string GenerateToken(UserModel usr)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usr.Id.ToString()),
            }),
            Audience = "Yugen",
            Issuer = "Yugen",
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtToken), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
