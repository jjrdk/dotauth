namespace DotAuth.Shared.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IClientAssertionJtiStore
{
    Task<bool> TryAddAsync(string jti, DateTimeOffset expiresAt, CancellationToken cancellationToken);

    Task CleanupExpiredAsync(CancellationToken cancellationToken);
}
