using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IUserProvider _userProvider;
    private readonly YugenContext _db;

    private readonly byte[] _jwtToken;

    public UserService(YugenContext db, IOptions<EncryptionConfig> encryptionSettings, IOptions<ProviderConfig> providerSettings)
    {
        _db = db;

        _userProvider = new JellyfinUserService(providerSettings.Value.jellyfin_Url!, providerSettings.Value.jellyfin_ApiKey!);
        _jwtToken = Convert.FromBase64String(encryptionSettings.Value.jwtToken);
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        return await _userProvider.GetAllUsers();
    }

    public async Task<UserSession?> LoginUser(string username, string password)
    {
        (object providerSession, ExternalUser externalUser)? res = await _userProvider.LoginUser(username, password);

        if (res == null)
            return null;

        UserModel? dbUser = _db.user.FirstOrDefault(x => x.ProviderId == res.Value.externalUser.ExternalId);

        if (dbUser == null)
        {
            await _db.AddAsync(new UserModel()
            {
                ProviderId = res.Value.externalUser.ExternalId
            });

            await _db.SaveChangesAsync();
            dbUser = _db.user.FirstOrDefault(x => x.ProviderId == res.Value.externalUser.ExternalId);
        }

        return new UserSession()
        {
            User = dbUser!,
            AccessToken = GenerateToken(dbUser!),
            ProviderSession = res.Value.providerSession,
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
