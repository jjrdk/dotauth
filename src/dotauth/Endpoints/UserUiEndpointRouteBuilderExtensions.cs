namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the authenticated user profile UI.
/// </summary>
public static class UserUiEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the user/profile UI endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapUserUiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/user", UserUiEndpointHandlers.GetIndex).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/consent", UserUiEndpointHandlers.GetConsent).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapPost("/user/consent", UserUiEndpointHandlers.PostConsent).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/edit", UserUiEndpointHandlers.Edit).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/updatecredentials", UserUiEndpointHandlers.Edit).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/updatetwofactor", UserUiEndpointHandlers.Edit).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapPost("/user/updatecredentials", UserUiEndpointHandlers.UpdateCredentials).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapPost("/user/updatetwofactor", UserUiEndpointHandlers.UpdateTwoFactor).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapPost("/user/link", UserUiEndpointHandlers.Link).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/linkcallback", UserUiEndpointHandlers.LinkCallback).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/linkprofileconfirmation", UserUiEndpointHandlers.LinkProfileConfirmation).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapGet("/user/confirmprofilelinking", UserUiEndpointHandlers.ConfirmProfileLinking).RequireAuthorization("authenticated").WithOrder(-1);
        endpoints.MapPost("/user/unlink", UserUiEndpointHandlers.Unlink).RequireAuthorization("authenticated").WithOrder(-1);
        return endpoints;
    }
}

