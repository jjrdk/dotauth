namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for scopes management.
/// </summary>
public static class ScopesEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the scopes management endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapScopesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CoreConstants.EndPoints.Scopes + "/.search", ScopesEndpointHandlers.Search)
            .RequireAuthorization("manager");

        endpoints.MapGet(CoreConstants.EndPoints.Scopes, ScopesEndpointHandlers.GetAll)
            .RequireAuthorization("manager");

        endpoints.MapGet(CoreConstants.EndPoints.Scopes + "/{id}", ScopesEndpointHandlers.Get)
            .RequireAuthorization("manager");

        endpoints.MapDelete(CoreConstants.EndPoints.Scopes + "/{id}", ScopesEndpointHandlers.Delete)
            .RequireAuthorization("manager");

        endpoints.MapPost(CoreConstants.EndPoints.Scopes, ScopesEndpointHandlers.Add)
            .RequireAuthorization("manager");

        endpoints.MapPost(CoreConstants.EndPoints.Scopes + "/{name}", ScopesEndpointHandlers.UpdateByName)
            .RequireAuthorization("manager");

        endpoints.MapPut(CoreConstants.EndPoints.Scopes, ScopesEndpointHandlers.Update)
            .RequireAuthorization("manager");

        return endpoints;
    }
}

