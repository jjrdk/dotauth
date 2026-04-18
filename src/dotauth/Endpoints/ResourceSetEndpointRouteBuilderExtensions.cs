namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for UMA resource sets.
/// </summary>
public static class ResourceSetEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the UMA resource set endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapResourceSetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{UmaConstants.RouteValues.ResourceSet}/.search", ResourceSetEndpointHandlers.SearchResourceSets)
            .RequireAuthorization("UmaProtection");
        endpoints.MapGet(UmaConstants.RouteValues.ResourceSet, ResourceSetEndpointHandlers.GetResourceSets)
            .RequireAuthorization("UmaProtection");
        endpoints.MapGet($"{UmaConstants.RouteValues.ResourceSet}/{{id}}", ResourceSetEndpointHandlers.GetResourceSet)
            .RequireAuthorization("UmaProtection");
        endpoints.MapGet($"{UmaConstants.RouteValues.ResourceSet}/{{id}}/policy", ResourceSetEndpointHandlers.GetResourceSetPolicy)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPost($"{UmaConstants.RouteValues.ResourceSet}/{{id}}/policy", ResourceSetEndpointHandlers.SetResourceSetPolicyFromViewModel)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPut($"{UmaConstants.RouteValues.ResourceSet}/{{id}}/policy", ResourceSetEndpointHandlers.SetResourceSetPolicy)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPost(UmaConstants.RouteValues.ResourceSet, ResourceSetEndpointHandlers.AddResourceSet)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPut(UmaConstants.RouteValues.ResourceSet, ResourceSetEndpointHandlers.UpdateResourceSet)
            .RequireAuthorization("UmaProtection");
        endpoints.MapDelete($"{UmaConstants.RouteValues.ResourceSet}/{{id}}", ResourceSetEndpointHandlers.DeleteResourceSet)
            .RequireAuthorization("UmaProtection");
        return endpoints;
    }
}


