namespace DotAuth.Telemetry;

using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Models;

/// <summary>
/// Wraps a token store so every store call emits latency telemetry.
/// </summary>
internal sealed class TelemetryTokenStore : ITokenStore
{
    private readonly ITokenStore _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryTokenStore"/> class.
    /// </summary>
    /// <param name="inner">The underlying token store.</param>
    public TelemetryTokenStore(ITokenStore inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public Task<GrantedToken?> GetToken(
        string scopes,
        string clientId,
        JwtPayload? idTokenJwsPayload = null,
        JwtPayload? userInfoJwsPayload = null,
        CancellationToken cancellationToken = default)
    {
        return TrackLookupAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreGet,
            operation: "get_valid_token",
            () => _inner.GetToken(scopes, clientId, idTokenJwsPayload, userInfoJwsPayload, cancellationToken));
    }

    /// <inheritdoc />
    public Task<GrantedToken?> GetRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        return TrackLookupAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreGet,
            operation: "get_refresh_token",
            () => _inner.GetRefreshToken(refreshToken, cancellationToken));
    }

    /// <inheritdoc />
    public Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        return TrackLookupAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreGet,
            operation: "get_access_token",
            () => _inner.GetAccessToken(accessToken, cancellationToken));
    }

    /// <inheritdoc />
    public Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
    {
        return TrackBooleanAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreAdd,
            operation: "add",
            () => _inner.AddToken(grantedToken, cancellationToken));
    }

    /// <inheritdoc />
    public Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        return TrackBooleanAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreRemove,
            operation: "remove_refresh",
            () => _inner.RemoveRefreshToken(refreshToken, cancellationToken));
    }

    /// <inheritdoc />
    public Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        return TrackBooleanAsync(
            activityName: DotAuthTelemetry.ActivityNames.TokenStoreRemove,
            operation: "remove_access",
            () => _inner.RemoveAccessToken(accessToken, cancellationToken));
    }

    /// <summary>
    /// Tracks a token lookup and annotates whether it was a cache hit.
    /// </summary>
    /// <param name="activityName">The span name.</param>
    /// <param name="operation">The token-store operation name.</param>
    /// <param name="executeAsync">The wrapped operation.</param>
    /// <returns>The token returned by the underlying store.</returns>
    private static async Task<GrantedToken?> TrackLookupAsync(
        string activityName,
        string operation,
        Func<Task<GrantedToken?>> executeAsync)
    {
        using var activity = DotAuthTelemetry.StartInternalActivity(activityName);
        activity?.SetTag(DotAuthTelemetry.TagKeys.TokenStoreOperation, operation);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var token = await executeAsync().ConfigureAwait(false);
            var hit = token is not null;
            activity?.SetTag(DotAuthTelemetry.TagKeys.TokenStoreHit, hit);
            activity?.SetStatus(ActivityStatusCode.Ok);
            DotAuthTelemetry.RecordTokenStoreOperationDuration(operation, stopwatch.Elapsed.TotalMilliseconds, hit);
            return token;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddEvent(new ActivityEvent("exception"));
            DotAuthTelemetry.RecordTokenStoreOperationDuration(operation, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Tracks a token-store mutation and records whether it succeeded.
    /// </summary>
    /// <param name="activityName">The span name.</param>
    /// <param name="operation">The token-store operation name.</param>
    /// <param name="executeAsync">The wrapped operation.</param>
    /// <returns>The underlying success flag.</returns>
    private static async Task<bool> TrackBooleanAsync(
        string activityName,
        string operation,
        Func<Task<bool>> executeAsync)
    {
        using var activity = DotAuthTelemetry.StartInternalActivity(activityName);
        activity?.SetTag(DotAuthTelemetry.TagKeys.TokenStoreOperation, operation);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var success = await executeAsync().ConfigureAwait(false);
            activity?.SetTag(DotAuthTelemetry.TagKeys.TokenStoreSuccess, success);
            activity?.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
            DotAuthTelemetry.RecordTokenStoreOperationDuration(operation, stopwatch.Elapsed.TotalMilliseconds);
            return success;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddEvent(new ActivityEvent("exception"));
            DotAuthTelemetry.RecordTokenStoreOperationDuration(operation, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}


