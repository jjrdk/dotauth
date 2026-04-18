namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings related to discovery (OpenID Connect/.well-known configuration) and UMA configuration.
/// </summary>
public static class DiscoveryEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapDiscoveryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(CoreConstants.EndPoints.DiscoveryAction, DiscoveryEndpointHandlers.GetDiscovery);
        return endpoints;
    }

    public static IEndpointRouteBuilder MapUmaConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(UmaConstants.RouteValues.Configuration, DiscoveryEndpointHandlers.GetUmaConfiguration);
        return endpoints;
    }
}


