namespace DotAuth.Api.Token.Actions;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.JwtToken;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Telemetry;
using Microsoft.Extensions.Logging;

internal sealed class GetTokenByDeviceAuthorizationTypeAction
{
    private readonly IDeviceAuthorizationStore _deviceAuthorizationStore;
    private readonly ITokenStore _tokenStore;
    private readonly IJwksStore _jwksStore;
    private readonly IClientStore _clientStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger _logger;

    public GetTokenByDeviceAuthorizationTypeAction(
        IDeviceAuthorizationStore deviceAuthorizationStore,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IClientStore clientStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _deviceAuthorizationStore = deviceAuthorizationStore;
        _tokenStore = tokenStore;
        _jwksStore = jwksStore;
        _clientStore = clientStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Option<GrantedToken>> Execute(
        string clientId,
        string deviceCode,
        string issuerName,
        CancellationToken cancellationToken)
    {
        using var activity = DotAuthTelemetry.StartInternalActivity("dotauth.token.device_code");
        activity?.SetTag("dotauth.client_id", DotAuthTelemetry.Normalize(clientId));
        var option = await _deviceAuthorizationStore.Get(clientId, deviceCode, cancellationToken).ConfigureAwait(false);
        if (option is Option<DeviceAuthorizationData>.Error e)
        {
            activity?.SetStatus(ActivityStatusCode.Error, e.Details.Title);
            return e.Details;
        }

        var authRequest = ((Option<DeviceAuthorizationData>.Result)option).Item;
        activity?.SetTag("dotauth.device_code.poll_interval_seconds", authRequest.Interval);
        if (authRequest.Approved)
        {
            activity?.SetTag("dotauth.device_code.status", "approved");
            DotAuthTelemetry.RecordDeviceCodePoll(authRequest.ClientId, "approved");
            var token = await HandleApprovedRequest(issuerName, authRequest, cancellationToken).ConfigureAwait(false);
            await _deviceAuthorizationStore.Remove(authRequest, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return token;
        }

        var now = DateTimeOffset.UtcNow;

        if (authRequest.Expires < now)
        {
            await _deviceAuthorizationStore.Remove(authRequest, cancellationToken).ConfigureAwait(false);
            const string format = "Device code {0} is expired at {1}";
            _logger.LogInformation(format, authRequest.DeviceCode, now);
            activity?.SetTag("dotauth.device_code.status", "expired");
            activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.ExpiredToken);
            DotAuthTelemetry.RecordDeviceCodePoll(authRequest.ClientId, "expired");
            return new ErrorDetails
            {
                Title = ErrorCodes.ExpiredToken,
                Detail = string.Format(format, authRequest.DeviceCode, now),
                Status = HttpStatusCode.BadRequest
            };
        }

        var lastPolled = authRequest.LastPolled;
        authRequest.LastPolled = now;
        await _deviceAuthorizationStore.Save(authRequest, cancellationToken).ConfigureAwait(false);
        if (lastPolled.AddSeconds(authRequest.Interval) <= now)
        {
            activity?.SetTag("dotauth.device_code.status", "pending");
            DotAuthTelemetry.RecordDeviceCodePoll(authRequest.ClientId, "pending");
            return new ErrorDetails
            {
                Title = ErrorCodes.AuthorizationPending,
                Detail = ErrorCodes.AuthorizationPending,
                Status = HttpStatusCode.BadRequest
            };
        }

        const string detail = "Device with client id {0} polled after only {1} seconds.";

        var totalSeconds = (now - lastPolled).TotalSeconds;
        _logger.LogInformation(detail, authRequest.ClientId, totalSeconds);
        activity?.SetTag("dotauth.device_code.status", "slow_down");
        activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.SlowDown);
        DotAuthTelemetry.RecordDeviceCodePoll(authRequest.ClientId, "slow_down");

        return new ErrorDetails
        {
            Title = ErrorCodes.SlowDown,
            Detail = string.Format(detail, authRequest.DeviceCode, totalSeconds),
            Status = HttpStatusCode.BadRequest
        };
    }

    private async Task<Option<GrantedToken>> HandleApprovedRequest(
        string issuerName,
        DeviceAuthorizationData authRequest,
        CancellationToken cancellationToken)
    {
        var scopes = string.Join(" ", authRequest.Scopes);
        var grantedToken = await _tokenStore.GetValidGrantedToken(
                _jwksStore,
                scopes,
                authRequest.ClientId,
                issuer: issuerName,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (grantedToken != null)
        {
            return grantedToken;
        }

        var client = await _clientStore.GetById(authRequest.ClientId, cancellationToken).ConfigureAwait(false);
        grantedToken = await client!.GenerateToken(
                _jwksStore,
                authRequest.Scopes,
                issuerName,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        await _eventPublisher.Publish(
                new TokenGranted(
                    Id.Create(),
                    grantedToken.UserInfoPayLoad?.Sub,
                    authRequest.ClientId,
                    string.Join(" ", authRequest.Scopes),
                    GrantTypes.AuthorizationCode,
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        // Fill-in the id-token
        if (grantedToken.IdTokenPayLoad != null)
        {
            grantedToken = grantedToken with
            {
                IdTokenPayLoad =
                JwtGenerator.UpdatePayloadDate(grantedToken.IdTokenPayLoad, client!.TokenLifetime),
                IdToken = await client.GenerateIdToken(
                        grantedToken.IdTokenPayLoad,
                        _jwksStore,
                        cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);

        return grantedToken;
    }
}
