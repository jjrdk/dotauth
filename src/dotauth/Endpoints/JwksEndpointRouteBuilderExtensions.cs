namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for JWKS operations.
/// </summary>
public static class JwksEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapJwksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.Jwks, JwksEndpointHandlers.GetJwks);
        endpoints.MapPost(CoreConstants.EndPoints.Jwks, JwksEndpointHandlers.AddJwk)
            .RequireAuthorization("manager");
        endpoints.MapPut(CoreConstants.EndPoints.Jwks, JwksEndpointHandlers.RotateJwks)
            .RequireAuthorization("manager");

        return endpoints;
    }
}


