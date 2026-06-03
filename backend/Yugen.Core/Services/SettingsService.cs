using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Helpers;
using Yugen.Data;
using Yugen.Domain.Data.Users;
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

    Radarr_Url,
    Radarr_ApiKey,

    BuildNumber,
    CommitSha,
}

public class SettingsService
{
    private readonly YugenContext _db;
    private readonly SettingsCache _cache;
    private readonly EndpointDeduplicator _endpointDeduplicator;

    public SettingsCache getCache => _cache;

    public static string GetCacheKey(ConfigKeys key) => $"SETTINGSCACHE_{key}";

    public SettingsService(YugenContext db, EndpointDeduplicator endpointDeduplicator, SettingsCache cache)
    {
        _db = db;
        _cache = cache;

        _endpointDeduplicator = endpointDeduplicator;
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

    public async Task OnLoad()
    {
        await RecacheAll();

        string? commit = Environment.GetEnvironmentVariable("GIT_COMMIT");
        string? build = Environment.GetEnvironmentVariable("BUILD_NUMBER");

        if (string.IsNullOrEmpty(commit) || string.IsNullOrEmpty(build))
            return;

        string currentCommit = _cache.Get(ConfigKeys.CommitSha);

        if (currentCommit != commit)
        {
            List<Model_Notification> notifications = new List<Model_Notification>();
            Guid[] users = await _db.user.Select(u => u.Id).ToArrayAsync();

            foreach (Guid usr in users)
                notifications.Add(new Model_Notification()
                {
                    Date = DateTime.UtcNow,
                    EventName = "Update",
                    UserId = usr,
                    Source = "System",
                    Message = $"v{build}",
                });

            await _db.AddRangeAsync(notifications);
            await SetConfigValue(ConfigKeys.CommitSha, commit);
            await SetConfigValue(ConfigKeys.BuildNumber, build);
        }
    }

    public async Task TriggerUpdate(UserSession usr)
    {
        using var _ = _endpointDeduplicator.TryAcquire(usr, nameof(TriggerUpdate));

        string containerId = File.ReadAllText("/etc/hostname").Trim();

        Process labelProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "docker",
                Arguments = $"inspect --format {{{{.Config.Labels.com.docker.compose.service}}}} {containerId}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        labelProcess.Start();
        string containerName = labelProcess.StandardOutput.ReadToEnd().Trim();
        labelProcess.WaitForExit();

        if (string.IsNullOrEmpty(containerName))
            throw new Exception("Couldnt determine container name");

        Console.WriteLine("Attempting pull of - " + containerName);
        ProcessStartInfo StartInfo = new ProcessStartInfo()
        {
            FileName = "docker",
            UseShellExecute = false
        };

        StartInfo.ArgumentList.Add("compose");
        StartInfo.ArgumentList.Add("up");
        StartInfo.ArgumentList.Add("-d");
        StartInfo.ArgumentList.Add("--pull");
        StartInfo.ArgumentList.Add("always");
        StartInfo.ArgumentList.Add("--no-deps");
        StartInfo.ArgumentList.Add(containerName);

        var p = new Process() { StartInfo = StartInfo };

        p.Start();
        p.WaitForExit();
    }
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