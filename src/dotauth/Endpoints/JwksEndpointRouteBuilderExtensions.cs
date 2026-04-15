namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for JWKS operations.
/// </summary>
public static class JwksEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the JWKS endpoint for retrieving the current set of JSON Web Keys, adding a new JWK, and rotating keys.
    /// </summary>
    /// <param name="endpoints">The endpoint builder</param>
    /// <returns></returns>
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


