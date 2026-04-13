namespace DotAuth.Endpoints;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

internal static class SessionEndpointHandlers
{
    internal static IResult CheckSession()
    {
        return Results.Json(new CheckSessionResponse { CookieName = CoreConstants.SessionId });
    }

    internal static async Task<IResult> RevokeSessionCallback(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IJwksStore jwksStore,
        IClientStore clientStore,
        CancellationToken cancellationToken)
    {
        var request = EndpointHandlerHelpers.BindFromQuery<RevokeSessionRequest>(httpContext.Request);
        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.CookieName)
            .ConfigureAwait(false);
        if (authenticatedUser == null || authenticatedUser.Identity?.IsAuthenticated != true)
        {
            return Results.Json(new { Authenticated = false });
        }

        httpContext.Response.Cookies.Delete(CoreConstants.SessionId);
        await authenticationService.SignOutAsync(httpContext, CookieNames.CookieName, new AuthenticationProperties())
            .ConfigureAwait(false);
        if (request == null
         || request.post_logout_redirect_uri == null
         || string.IsNullOrWhiteSpace(request.id_token_hint))
        {
            return Results.Json(new { Authenticated = true });
        }

        var handler = new JwtSecurityTokenHandler();
        var jsonWebKeySet = await jwksStore.GetPublicKeys(cancellationToken).ConfigureAwait(false);
        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = jsonWebKeySet.Keys
        };
        handler.ValidateToken(request.id_token_hint, tokenValidationParameters, out var token);
        var jws = (token as JwtSecurityToken)?.Payload;
        var claim = jws?.GetClaimValue(StandardClaimNames.Azp);
        if (claim == null)
        {
            return Results.Json(new { Authenticated = true });
        }

        var client = await clientStore.GetById(claim, cancellationToken).ConfigureAwait(false);
        if (client?.PostLogoutRedirectUris == null || client.PostLogoutRedirectUris.All(x => x != request.post_logout_redirect_uri))
        {
            return Results.Json(new { Authenticated = true });
        }

        var redirectUrl = request.post_logout_redirect_uri;
        if (!string.IsNullOrWhiteSpace(request.state))
        {
            redirectUrl = new Uri($"{redirectUrl.AbsoluteUri}?state={request.state}");
        }

        return Results.Redirect(redirectUrl.AbsoluteUri);
    }
}



