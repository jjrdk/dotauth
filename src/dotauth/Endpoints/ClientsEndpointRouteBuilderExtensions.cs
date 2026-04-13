namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for client management and dynamic client registration.
/// </summary>
public static class ClientsEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the client endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapClientsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CoreConstants.EndPoints.Clients + "/register", ClientsEndpointHandlers.Register)
            .RequireAuthorization("dcr");
        endpoints.MapPut(CoreConstants.EndPoints.Clients + "/register/{clientId}", ClientsEndpointHandlers.Modify)
            .RequireAuthorization("dcr");

        endpoints.MapGet(CoreConstants.EndPoints.Clients, ClientsEndpointHandlers.GetAll)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.Clients + "/.search", ClientsEndpointHandlers.Search)
            .RequireAuthorization("manager");
        endpoints.MapGet(CoreConstants.EndPoints.Clients + "/create", ClientsEndpointHandlers.Create)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.Clients + "/create", ClientsEndpointHandlers.CreatePost)
            .RequireAuthorization("manager");
        endpoints.MapGet(CoreConstants.EndPoints.Clients + "/{id}", ClientsEndpointHandlers.Get)
            .RequireAuthorization("manager");
        endpoints.MapDelete(CoreConstants.EndPoints.Clients + "/{id}", ClientsEndpointHandlers.Delete)
            .RequireAuthorization("manager");
        endpoints.MapPut(CoreConstants.EndPoints.Clients, ClientsEndpointHandlers.Put)
            .RequireAuthorization("manager");
        endpoints.MapPost(CoreConstants.EndPoints.Clients, ClientsEndpointHandlers.Add)
            .RequireAuthorization("manager");
        return endpoints;
    }
}

