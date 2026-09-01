namespace DotAuth.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Caching.Distributed;

internal sealed class DistributedClientAssertionJtiStore : IClientAssertionJtiStore
{
    private const string Prefix = "client-assertion-jti:";
    private readonly IDistributedCache _cache;

    public DistributedClientAssertionJtiStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> TryAddAsync(string jti, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = Prefix + jti;
        var existing = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return false;
        }

        var ttl = expiresAt - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(1);
        }

        await _cache.SetStringAsync(
                key,
                "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt
                },
                cancellationToken)
            .ConfigureAwait(false);
        return true;
    }

    public Task CleanupExpiredAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
