namespace DotAuth.Telemetry;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Wraps a JWK repository so key retrieval emits tracing telemetry.
/// </summary>
internal sealed class TelemetryJwksRepository : IJwksRepository
{
    private readonly IJwksRepository _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryJwksRepository"/> class.
    /// </summary>
    /// <param name="inner">The underlying JWK repository.</param>
    public TelemetryJwksRepository(IJwksRepository inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public Task<JsonWebKeySet> GetPublicKeys(CancellationToken cancellationToken = default)
    {
        return TrackAsync(
            "dotauth.jwks.get_public_keys",
            algorithm: null,
            executeAsync: () => _inner.GetPublicKeys(cancellationToken),
            configure: keySet =>
            {
                var keyCount = keySet.Keys?.Count ?? 0;
                return new[] { new KeyValuePair<string, object?>("dotauth.jwks.key_count", keyCount) };
            });
    }

    /// <inheritdoc />
    public Task<SigningCredentials?> GetSigningKey(string alg, CancellationToken cancellationToken = default)
    {
        return TrackAsync(
            "dotauth.jwks.get_signing_key",
            alg,
            () => _inner.GetSigningKey(alg, cancellationToken),
            credentials => BuildKeyTags(credentials?.Key));
    }

    /// <inheritdoc />
    public Task<SecurityKey?> GetEncryptionKey(string alg, CancellationToken cancellationToken = default)
    {
        return TrackAsync(
            "dotauth.jwks.get_encryption_key",
            alg,
            () => _inner.GetEncryptionKey(alg, cancellationToken),
            BuildKeyTags);
    }

    /// <inheritdoc />
    public Task<SigningCredentials?> GetDefaultSigningKey(CancellationToken cancellationToken = default)
    {
        return TrackAsync(
            "dotauth.jwks.get_signing_key",
            "default",
            () => _inner.GetDefaultSigningKey(cancellationToken),
            credentials => BuildKeyTags(credentials?.Key));
    }

    /// <inheritdoc />
    public Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default)
    {
        return _inner.Add(key, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default)
    {
        return _inner.Rotate(keySet, cancellationToken);
    }

    /// <summary>
    /// Tracks a JWK operation and enriches the span with key metadata when available.
    /// </summary>
    /// <typeparam name="TResult">The operation result type.</typeparam>
    /// <param name="activityName">The span name.</param>
    /// <param name="algorithm">The requested algorithm.</param>
    /// <param name="executeAsync">The wrapped repository call.</param>
    /// <param name="configure">Adds result-specific telemetry tags.</param>
    /// <returns>The wrapped repository result.</returns>
    private static async Task<TResult> TrackAsync<TResult>(
        string activityName,
        string? algorithm,
        Func<Task<TResult>> executeAsync,
        Func<TResult, KeyValuePair<string, object?>[]> configure)
    {
        using var activity = DotAuthTelemetry.StartInternalActivity(activityName);
        activity?.SetTag("dotauth.jwks.algorithm", DotAuthTelemetry.Normalize(algorithm));
        try
        {
            var result = await executeAsync().ConfigureAwait(false);
            foreach (var (key, value) in configure(result))
            {
                activity?.SetTag(key, value);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddEvent(new ActivityEvent("exception"));
            throw;
        }
    }

    /// <summary>
    /// Builds stable telemetry tags from a returned key.
    /// </summary>
    /// <param name="key">The returned security key.</param>
    /// <returns>The telemetry tags to apply.</returns>
    private static KeyValuePair<string, object?>[] BuildKeyTags(SecurityKey? key)
    {
        return
        [
            new KeyValuePair<string, object?>("dotauth.jwks.key_id", DotAuthTelemetry.Normalize(key?.KeyId)),
            new KeyValuePair<string, object?>("dotauth.jwks.key_found", key is not null)
        ];
    }
}



