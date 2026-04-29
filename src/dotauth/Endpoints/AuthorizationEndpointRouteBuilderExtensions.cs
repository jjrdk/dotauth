namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the authorization endpoint.
/// </summary>
public static class AuthorizationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the authorization endpoint. Supports both GET (query string) and POST
    /// (form-encoded body) as required by RFC 6749 section 3.1 and OpenID Connect Core.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.Authorization, AuthorizationEndpointHandlers.Get);
        // POST support allows clients to send authorization requests via form body,
        // preventing sensitive parameters from appearing in server logs or browser history.
        endpoints.MapPost(CoreConstants.EndPoints.Authorization, AuthorizationEndpointHandlers.Post);
        return endpoints;
    }
}
