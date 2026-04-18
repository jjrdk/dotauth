namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for device authorization endpoint.
/// </summary>
public static class DeviceAuthorizationEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapDeviceAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CoreConstants.EndPoints.DeviceAuthorization, DeviceAuthorizationEndpointHandlers.RequestDeviceAuthorization);
        return endpoints;
    }
}


