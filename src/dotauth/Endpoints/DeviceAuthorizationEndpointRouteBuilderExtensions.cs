namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for device authorization endpoint.
/// </summary>
public static class DeviceAuthorizationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the device authorization endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint builder</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapDeviceAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CoreConstants.EndPoints.DeviceAuthorization, DeviceAuthorizationEndpointHandlers.RequestDeviceAuthorization);
        return endpoints;
    }
}


