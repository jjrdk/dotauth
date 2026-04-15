namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for token and revocation endpoints.
/// </summary>
public static class TokenEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the token endpoint for exchanging authorization grants for access tokens, and the revocation endpoint for revoking access tokens and refresh tokens.
    /// </summary>
    /// <param name="endpoints">The endpoint builder</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(UmaConstants.RouteValues.Token, TokenEndpointHandlers.PostToken);
        endpoints.MapPost(CoreConstants.EndPoints.Revocation, TokenEndpointHandlers.RevokeToken);
        return endpoints;
    }
}


