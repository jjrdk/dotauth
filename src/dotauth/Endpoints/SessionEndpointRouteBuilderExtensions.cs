namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for session management.
/// </summary>
public static class SessionEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the check-session and end-session endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.CheckSession, SessionEndpointHandlers.CheckSession);
        endpoints.MapGet(CoreConstants.EndPoints.EndSession, SessionEndpointHandlers.RevokeSessionCallback);
        return endpoints;
    }
}

