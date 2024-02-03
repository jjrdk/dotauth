namespace DotAuth.Uma;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the claims principal extensions.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Checks whether the <see cref="ClaimsPrincipal"/> has access to the requested resource.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> seeking access.</param>
    /// <param name="registration">The <see cref="ResourceRegistration"/> to access.</param>
    /// <param name="scope">The access scope.</param>
    /// <returns></returns>
    public static bool CheckResourceAccess(
        this ClaimsPrincipal principal,
        ResourceRegistration registration,
        params string[] scope)
    {
        return registration.Owner == principal.GetSubject()
         || principal.CheckResourceAccess(registration.ResourceSetId, scope);
    }

    /// <summary>
    /// Checks whether the <see cref="ClaimsPrincipal"/> has access to the requested resource.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> seeking access.</param>
    /// <param name="resourceSetId">The resource set to access.</param>
    /// <param name="scope">The access scope.</param>
    /// <returns></returns>
    public static bool CheckResourceAccess(
        this ClaimsPrincipal principal,
        string? resourceSetId,
        params string[] scope)
    {
        if (string.IsNullOrWhiteSpace(resourceSetId))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return principal.Identity is ClaimsIdentity identity
         && identity.TryGetUmaTickets(out var permissions)
         && permissions.Any(
                l => l.ResourceSetId == resourceSetId
                 && (l.NotBefore ?? 0) <= now
                 && (l.Expiry ?? long.MaxValue) > now
                 && scope.All(l.Scopes.Contains));
    }

    public static ClaimsPrincipal GetClaimsPrincipal(this GrantedTokenResponse rpt, JsonWebKeySet jwks)
    {
        var handler = new JwtSecurityTokenHandler();
        var signingKeys = jwks.GetSigningKeys();
        var tokenValidationParameters = new TokenValidationParameters
        {
            SignatureValidator = (token, _) => handler.ReadJwtToken(token),
            IssuerSigningKeys = signingKeys,
            ValidateActor = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateTokenReplay = false
        };

        var principal = handler.ValidateToken(rpt.AccessToken, tokenValidationParameters, out _);
        return principal;
    }
}
