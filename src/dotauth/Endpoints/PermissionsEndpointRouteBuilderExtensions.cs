namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for UMA permission requests.
/// </summary>
public static class PermissionsEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the UMA permission endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapPermissionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(UmaConstants.RouteValues.Permission, PermissionsEndpointHandlers.GetPermissionRequests)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPost($"{UmaConstants.RouteValues.Permission}/{{id}}/approve", PermissionsEndpointHandlers.ApprovePermissionRequest)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPost(UmaConstants.RouteValues.Permission, PermissionsEndpointHandlers.RequestPermission)
            .RequireAuthorization("UmaProtection");
        endpoints.MapPost($"{UmaConstants.RouteValues.Permission}/bulk", PermissionsEndpointHandlers.BulkRequestPermissions)
            .RequireAuthorization("UmaProtection");
        return endpoints;
    }
}


