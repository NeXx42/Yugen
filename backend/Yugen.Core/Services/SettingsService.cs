using Microsoft.EntityFrameworkCore;
using Yugen.Data;
using Yugen.Domain.Models;

namespace Yugen.Core.Services;

public enum ConfigKeys
{
    Jellyfin_Url,
    Jellyfin_ApiKey,

    IdMoe_Url,
    IdMoe_ApiKey,

    Jikan_Url,
    Jikan_ApiKey,

    Sonarr_Url,
    Sonarr_ApiKey,

    AdultContent,
    HasSearchCriteriaCached,
}

public class SettingsService
{
    private readonly YugenContext _db;
    private readonly SettingsCache _cache;

    public SettingsCache getCache => _cache;

    public static string GetCacheKey(ConfigKeys key) => $"SETTINGSCACHE_{key}";

    public SettingsService(YugenContext db, SettingsCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task SetConfigValue(ConfigKeys key, bool? value)
        => await SetConfigValue(key, value.HasValue ? (value.Value ? "1" : "0") : null);

    public async Task SetConfigValue(ConfigKeys key, string? value)
    {
        var existing = await _db.config.Where(c => c.Key == key.ToString()).ToListAsync();

        if (existing.Count > 0)
            _db.RemoveRange(existing);

        _cache.Remove(key);

        if (string.IsNullOrEmpty(value))
        {
            await _db.SaveChangesAsync();
            return;
        }

        _db.Add(new Model_Config()
        {
            Key = key.ToString(),
            Value = value
        });

        await _db.SaveChangesAsync();
        _cache.Set(key, value);
    }

    public async Task<Dictionary<ConfigKeys, string?>> RecacheAll()
    {
        _cache.Clear();

        ConfigKeys[] keys = (ConfigKeys[])Enum.GetValues(typeof(ConfigKeys));
        Model_Config[] configValues = await _db.config.Where(c => keys.Select(k => k.ToString()).Contains(c.Key)).ToArrayAsync();

        Dictionary<ConfigKeys, string?> res = new Dictionary<ConfigKeys, string?>();

        foreach (ConfigKeys key in keys)
        {
            Model_Config? setting = configValues.FirstOrDefault(c => c.Key == key.ToString());

            res[key] = setting?.Value;
            _cache.Set(key, setting?.Value);
        }

        return res;
    }

    public async Task<Dictionary<ConfigKeys, string?>> GetAllCache() => _cache.cache;
}

public class SettingsCache
{
    public Dictionary<ConfigKeys, string?> cache = new Dictionary<ConfigKeys, string?>();

    public void Set(ConfigKeys key, string? val) => cache[key] = val;
    public void Remove(ConfigKeys key) => cache.Remove(key);

    public string Get(ConfigKeys key, string fallback = "")
    {
        if (cache.TryGetValue(key, out string? res))
            return res ?? fallback;

        return fallback;
    }

    public bool Get(ConfigKeys key, bool fallback) => Get(key, fallback ? "1" : "0") == "1";
    public void Clear() => cache.Clear();
}