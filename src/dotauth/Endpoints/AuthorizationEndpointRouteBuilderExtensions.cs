namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the authorization endpoint.
/// </summary>
public static class AuthorizationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the authorization endpoint.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.Authorization, AuthorizationEndpointHandlers.Get);
        return endpoints;
    }
}

