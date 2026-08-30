namespace DotAuth.Repositories;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Repositories;

internal sealed class InMemoryClientAssertionJtiStore : IClientAssertionJtiStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _entries = new(StringComparer.Ordinal);

    public Task<bool> TryAddAsync(string jti, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CleanupExpired(DateTimeOffset.UtcNow);
        return Task.FromResult(_entries.TryAdd(jti, expiresAt));
    }

    public Task CleanupExpiredAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CleanupExpired(DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    private void CleanupExpired(DateTimeOffset now)
    {
        foreach (var entry in _entries)
        {
            if (entry.Value <= now)
            {
                _entries.TryRemove(entry.Key, out _);
            }
        }
    }
}
