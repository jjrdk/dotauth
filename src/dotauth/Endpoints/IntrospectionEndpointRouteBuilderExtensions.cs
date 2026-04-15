namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for token introspection.
/// </summary>
public static class IntrospectionEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the token introspection endpoint and UMA token introspection endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint builder</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapIntrospectionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CoreConstants.EndPoints.Introspection, IntrospectionEndpointHandlers.PostIntrospection)
            .RequireAuthorization();
        endpoints.MapPost(UmaConstants.RouteValues.Introspection, IntrospectionEndpointHandlers.PostUmaIntrospection)
            .RequireAuthorization();
        return endpoints;
    }
}


