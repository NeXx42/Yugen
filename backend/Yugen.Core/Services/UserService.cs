using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models;
using Yugen.Domain.Models.User;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class UserService
{
    public const string CLAIM_ID = "ID";
    public const string CLAIM_JELLYFIN_ID = "JELLYFIN_ID";

    private readonly YugenContext _db;

    private readonly CacheService _cache;
    private readonly IUserProvider _userProvider;

    private readonly byte[] _jwtToken;

    public UserService(YugenContext db, IOptions<EncryptionConfig> encryptionSettings, SettingsCache settings, CacheService cache, ILogging logger)
    {
        _db = db;

        _cache = cache;

        _userProvider = new JellyfinUserService(settings.Get(ConfigKeys.Jellyfin_Url), settings.Get(ConfigKeys.Jellyfin_ApiKey), logger);
        _jwtToken = Convert.FromBase64String(encryptionSettings.Value.jwtToken);
    }

    public async Task<JWTUser> GetUser(Guid userId)
    {
        string CACHE_KEY = $"USER_SESSION_{userId}";

        if (_cache.TryGetValue(CACHE_KEY, out JWTUser? usr))
            return usr!;

        UserModel dbUser = await _db.user.SingleAsync(x => x.Id == userId);
        usr = new JWTUser()
        {
            Id = dbUser!.Id,
            JellyfinId = dbUser.ProviderId
        };

        _cache.SetIfNotExists(CACHE_KEY, usr);
        return usr;
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        return await _userProvider.GetAllUsers();
    }

    public async Task<string?> LoginUser(string username, string password)
    {
        string? jellyfinId = await _userProvider.LoginUser(username, password);

        if (string.IsNullOrEmpty(jellyfinId))
            return null;

        UserModel? dbUser = _db.user.FirstOrDefault(x => x.ProviderId == jellyfinId);

        if (dbUser == null)
        {
            dbUser = new UserModel()
            {
                ProviderId = jellyfinId
            };

            await _db.AddAsync(dbUser);
            await _db.SaveChangesAsync();
        }

        return GenerateToken(dbUser!);
    }

    public async Task<UserSession?> GetSession(JWTUser? usr)
    {
        if (usr == null)
            return null;

        UserModel? usrModel = await _db.user
            .Include(u => u.settings)
            .Where(u => u.Id == usr.Id)
            .SingleOrDefaultAsync();

        if (usrModel == null)
            return null;

        return new UserSession()
        {
            Id = usr.Id,
            Settings = usrModel.settings.ToDictionary(s => s.Key, s => s.Value)
        };
    }

    public async Task SaveUserSetting(Guid userId, string key, string? value)
    {
        UserModel? usr = await _db.user
            .Include(s => s.settings.Where(s => s.Key.Equals(key)))
            .Where(u => u.Id == userId)
            .SingleOrDefaultAsync();

        if (usr == null)
            throw new Exception("User doesnt exist");

        if (string.IsNullOrEmpty(value))
        {
            if (usr.settings.Count == 0)
                return;

            _db.RemoveRange(usr.settings);
        }
        else
        {
            if (usr.settings.Count == 0)
            {
                await _db.AddAsync(new Model_UserSetting
                {
                    UserId = userId,
                    Key = key,
                    Value = value
                });
            }
            else
            {
                usr.settings.Single().Value = value;
            }
        }

        await _db.SaveChangesAsync();
    }

    public string GenerateToken(UserModel usr)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(CLAIM_ID, usr.Id.ToString()),
                new Claim(CLAIM_JELLYFIN_ID, usr.ProviderId)
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
