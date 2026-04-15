namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the userinfo endpoint.
/// </summary>
public static class UserInfoEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the userinfo endpoint for retrieving claims about the authenticated user. The endpoint supports both GET and POST methods, as allowed by the OpenID Connect specification.
    /// </summary>
    /// <param name="endpoints">The endpoint builder</param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapUserInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods(
            CoreConstants.EndPoints.UserInfo,
            [HttpMethods.Get, HttpMethods.Post],
            UserInfoEndpointHandlers.GetUserInfo);
        return endpoints;
    }
}


