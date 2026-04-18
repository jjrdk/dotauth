namespace DotAuth.Endpoints;

using DotAuth.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the userinfo endpoint.
/// </summary>
public static class UserInfoEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapUserInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods(
            CoreConstants.EndPoints.UserInfo,
            new[] { HttpMethods.Get, HttpMethods.Post },
            UserInfoEndpointHandlers.GetUserInfo);
        return endpoints;
    }
}


