using System.Collections.Concurrent;
using Yugen.Domain.Data.Users;

namespace Yugen.Core.Helpers;

public class EndpointDeduplicator
{
    private readonly ConcurrentDictionary<string, byte> _activeRequests = new();

    public IDisposable? TryAcquire(UserSession session, string endpointName, params string[] extra)
    {
        string key = $"{session.JellyfinId}_{endpointName}_{string.Join("_", extra)}";

        if (!_activeRequests.TryAdd(key, 0))
            throw new UnauthorizedAccessException();

        return new Releaser(_activeRequests, key);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly ConcurrentDictionary<string, byte> _requests;
        private readonly string _key;

        public Releaser(ConcurrentDictionary<string, byte> requests, string key)
        {
            _requests = requests;
            _key = key;
        }

        public void Dispose()
        {
            _requests.TryRemove(_key, out _);
        }
    }
}
