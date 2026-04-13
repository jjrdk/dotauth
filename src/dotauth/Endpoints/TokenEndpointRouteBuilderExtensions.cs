namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for token and revocation endpoints.
/// </summary>
public static class TokenEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(UmaConstants.RouteValues.Token, TokenEndpointHandlers.PostToken);
        endpoints.MapPost(CoreConstants.EndPoints.Revocation, TokenEndpointHandlers.RevokeToken);
        return endpoints;
    }
}


