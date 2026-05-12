using System.Collections.Concurrent;

namespace Yugen.Core.Services;

public class CacheService
{
    private ConcurrentDictionary<string, CacheObject> cache = new ConcurrentDictionary<string, CacheObject>();

    public bool SetIfNotExists<T>(string set, T dat, TimeSpan? expire = null)
    {
        if (TryGetValue<T>(set, out _))
            return false;

        return cache.TryAdd(set, new CacheObject(dat, expire));
    }

    public bool TryGetValue<T>(string key, out T? dat)
    {
        if (cache.TryGetValue(key, out CacheObject o))
        {
            if (!o.IsValid())
            {

                cache.Remove(key, out _);

                dat = default;
                return false;
            }

            return o.GetValue(out dat);
        }

        dat = default;
        return false;
    }

    public bool Remove(string key)
        => cache.Remove(key, out _);

    public void Clear()
    {
        cache.Clear();
    }


    private struct CacheObject
    {
        public object? value;
        private DateTime? validUntil;

        public CacheObject(object? data, TimeSpan? expire)
        {
            value = data;

            if (expire.HasValue)
                validUntil = DateTime.UtcNow.Add(expire.Value);
        }

        public bool IsValid()
            => !validUntil.HasValue || DateTime.UtcNow < validUntil;

        public bool GetValue<T>(out T? val)
        {
            val = (T?)value;
            return true;
        }
    }
}