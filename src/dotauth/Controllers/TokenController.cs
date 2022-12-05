namespace DotAuth.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token;
using DotAuth.Common;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Policies;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Defines the token controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(UmaConstants.RouteValues.Token)]
[ThrottleFilter]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class TokenController : ControllerBase
{
    private readonly ILogger<TokenController> _logger;
    private readonly TokenActions _tokenActions;
    private readonly UmaTokenActions _umaTokenActions;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenController"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="scopeRepository">The scope repository.</param>
    /// <param name="resourceOwnerRepository"></param>
    /// <param name="authenticateResourceOwnerServices">The authenticate resource owner services.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="ticketStore">The ticket store.</param>
    /// <param name="jwksStore"></param>
    /// <param name="authorizationPolicyValidator">The authorization policy validator.</param>
    /// <param name="deviceAuthorizationStore">The device authorization store.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="logger">The logger.</param>
    public TokenController(
        RuntimeSettings settings,
        IAuthorizationCodeStore authorizationCodeStore,
        IClientStore clientStore,
        IScopeStore scopeRepository,
        IResourceOwnerRepository resourceOwnerRepository,
        IEnumerable<IAuthenticateResourceOwnerService> authenticateResourceOwnerServices,
        ITokenStore tokenStore,
        ITicketStore ticketStore,
        IJwksStore jwksStore,
        IAuthorizationPolicyValidator authorizationPolicyValidator,
        IDeviceAuthorizationStore deviceAuthorizationStore,
        IEventPublisher eventPublisher,
        ILogger<TokenController> logger)
    {
        _logger = logger;
        _tokenActions = new TokenActions(
            settings,
            authorizationCodeStore,
            clientStore,
            scopeRepository,
            jwksStore,
            resourceOwnerRepository,
            authenticateResourceOwnerServices,
            eventPublisher,
            tokenStore,
            deviceAuthorizationStore,
            logger);
        _umaTokenActions = new UmaTokenActions(
            ticketStore,
            settings,
            clientStore,
            scopeRepository,
            tokenStore,
            jwksStore,
            authorizationPolicyValidator,
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Handles the token request.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [HttpPost]
    public async Task<IActionResult> PostToken(
        [FromForm] TokenRequest tokenRequest,
        CancellationToken cancellationToken)
    {
        var certificate = Request.GetCertificate();
        if (tokenRequest.grant_type == null)
        {
            return BadRequest(
                new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidRequest,
                    Detail = string.Format(Strings.MissingParameter, RequestTokenNames.GrantType)
                });
        }

        AuthenticationHeaderValue? authenticationHeaderValue = null;
        if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
        {
            _ = AuthenticationHeaderValue.TryParse(authorizationHeader[0], out authenticationHeaderValue);
        }

        var issuerName = Request.GetAbsoluteUriWithVirtualPath();
        var result = await GetGrantedToken(
                tokenRequest,
                authenticationHeaderValue,
                certificate,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);

        if (result is Option<GrantedToken>.Result r)
        {
            return new OkObjectResult(r.Item.ToDto());
        }

        var e = (Option<GrantedToken>.Error)result;

        _logger.LogError(
            "Could not issue token. {title} - {detail} - {status}",
            e.Details.Title,
            e.Details.Detail,
            e.Details.Status);

        return new BadRequestObjectResult(e.Details);
    }

    private async Task<Option<GrantedToken>> GetGrantedToken(
        TokenRequest tokenRequest,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        switch (tokenRequest.grant_type)
        {
            case GrantTypes.Device:
                return await _tokenActions.GetTokenByDeviceGrantType(
                    tokenRequest.client_id,
                    tokenRequest.device_code,
                    issuerName,
                    cancellationToken).ConfigureAwait(false);
            case GrantTypes.Password:
                var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
                return await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(
                        resourceOwnerParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            case GrantTypes.AuthorizationCode:
                var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                return await _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        authCodeParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            case GrantTypes.RefreshToken:
                var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                return await _tokenActions.GetTokenByRefreshTokenGrantType(
                        refreshTokenParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            case GrantTypes.ClientCredentials:
                var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                return await _tokenActions.GetTokenByClientCredentialsGrantType(
                        clientCredentialsParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            case GrantTypes.UmaTicket:
                var tokenIdParameter = tokenRequest.ToTokenIdGrantTypeParameter();

                return await _umaTokenActions.GetTokenByTicketId(
                        tokenIdParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            //case GrantTypes.ValidateBearer:
            //return null;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenRequest));
        }
    }

    /// <summary>
    /// Handles the token revocation.
    /// </summary>
    /// <param name="revocationRequest">The revocation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost("revoke")]
    public async Task<IActionResult> PostRevoke(
        [FromForm] RevocationRequest revocationRequest,
        CancellationToken cancellationToken)
    {
        // 1. Fetch the authorization header
        AuthenticationHeaderValue? authenticationHeaderValue = null;
        if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
        {
            var authorizationHeaderValue = authorizationHeader.First();
            var splittedAuthorizationHeaderValue = authorizationHeaderValue!.Split(' ');
            if (splittedAuthorizationHeaderValue.Length == 2)
            {
                authenticationHeaderValue = new AuthenticationHeaderValue(
                    splittedAuthorizationHeaderValue[0],
                    splittedAuthorizationHeaderValue[1]);
            }
        }

        // 2. Revoke the token
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();
        var option = await _tokenActions.RevokeToken(
                revocationRequest.ToParameter(),
                authenticationHeaderValue,
                Request.GetCertificate(),
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        return option switch
        {
            Option.Success => new OkResult(),
            Option.Error e => BadRequest(e.Details),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}