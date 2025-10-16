namespace DotAuth.Controllers;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the session controller
/// </summary>
/// <seealso cref="ControllerBase" />
[ThrottleFilter]
public sealed class SessionController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwksStore _jwksStore;
    private readonly IClientStore _clientRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionController"/> class.
    /// </summary>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="jwksStore">The key store.</param>
    /// <param name="clientRepository">The client repository.</param>
    public SessionController(
        IAuthenticationService authenticationService,
        IJwksStore jwksStore,
        IClientStore clientRepository)
    {
        _authenticationService = authenticationService;
        _jwksStore = jwksStore;
        _clientRepository = clientRepository;
    }

    /// <summary>
    /// Checks the session.
    /// </summary>
    /// <returns></returns>
    [HttpGet(CoreConstants.EndPoints.CheckSession)]
    public Task<IActionResult> CheckSession()
    {
        return Task.FromResult<IActionResult>(Ok(new CheckSessionResponse{ CookieName = CoreConstants.SessionId }));
    }

    /// <summary>
    /// Handles the revoke session callback.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet(CoreConstants.EndPoints.EndSession)]
    public async Task<IActionResult> RevokeSessionCallback([FromQuery] RevokeSessionRequest? request, CancellationToken cancellationToken)
    {
        var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, CookieNames.CookieName).ConfigureAwait(false);
        if (authenticatedUser == null || authenticatedUser.Identity?.IsAuthenticated != true)
        {
            return Ok(new { Authenticated = false });
        }

        Response.Cookies.Delete(CoreConstants.SessionId);
        await _authenticationService.SignOutAsync(HttpContext, CookieNames.CookieName, new AuthenticationProperties()).ConfigureAwait(false);
        if (request != null
            && request.post_logout_redirect_uri != null
            && !string.IsNullOrWhiteSpace(request.id_token_hint))
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonWebKeySet = await _jwksStore.GetPublicKeys(cancellationToken).ConfigureAwait(false);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateActor = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKeys = jsonWebKeySet?.Keys
            };
            handler.ValidateToken(request.id_token_hint, tokenValidationParameters, out var token);
            var jws = (token as JwtSecurityToken)?.Payload;
            var claim = jws?.GetClaimValue(StandardClaimNames.Azp);
            if (claim != null)
            {
                var client = await _clientRepository.GetById(claim, cancellationToken).ConfigureAwait(false);
                if (client?.PostLogoutRedirectUris != null && client.PostLogoutRedirectUris.Any(x => x == request.post_logout_redirect_uri))
                {
                    var redirectUrl = request.post_logout_redirect_uri;
                    if (!string.IsNullOrWhiteSpace(request.state))
                    {
                        redirectUrl = new Uri($"{redirectUrl.AbsoluteUri}?state={request.state}");
                    }

                    return Redirect(redirectUrl.AbsoluteUri);
                }
            }
        }

        return Ok(new { Authenticated = true });
    }
}
