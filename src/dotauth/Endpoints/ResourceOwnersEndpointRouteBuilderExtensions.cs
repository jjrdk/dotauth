namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for resource owner management.
/// </summary>
public static class ResourceOwnersEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the resource owner endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapResourceOwnersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.ResourceOwners, ResourceOwnersEndpointHandlers.GetAll)
            .RequireAuthorization("manager");
        endpoints.MapGet(CoreConstants.EndPoints.ResourceOwners + "/{id}", ResourceOwnersEndpointHandlers.Get)
            .RequireAuthorization("manager");
        endpoints.MapDelete(CoreConstants.EndPoints.ResourceOwners + "/{id}", ResourceOwnersEndpointHandlers.Delete)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.ResourceOwners + "/{id}/delete", ResourceOwnersEndpointHandlers.Delete)
            .RequireAuthorization("manager");
        endpoints.MapDelete(CoreConstants.EndPoints.ResourceOwners, ResourceOwnersEndpointHandlers.DeleteMe)
            .RequireAuthorization();
        endpoints.MapPost(CoreConstants.EndPoints.ResourceOwners + "/{id}/update", ResourceOwnersEndpointHandlers.Update)
            .RequireAuthorization("manager");
        endpoints.MapPut(CoreConstants.EndPoints.ResourceOwners + "/claims", ResourceOwnersEndpointHandlers.UpdateClaims)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.ResourceOwners + "/claims", ResourceOwnersEndpointHandlers.UpdateMyClaims)
            .RequireAuthorization();
        endpoints.MapDelete(CoreConstants.EndPoints.ResourceOwners + "/claims", ResourceOwnersEndpointHandlers.DeleteMyClaims)
            .RequireAuthorization();
        endpoints.MapPut(CoreConstants.EndPoints.ResourceOwners + "/password", ResourceOwnersEndpointHandlers.UpdatePassword)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.ResourceOwners, ResourceOwnersEndpointHandlers.Add)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.ResourceOwners + "/.search", ResourceOwnersEndpointHandlers.Search)
            .RequireAuthorization("manager");
        return endpoints;
    }
}

