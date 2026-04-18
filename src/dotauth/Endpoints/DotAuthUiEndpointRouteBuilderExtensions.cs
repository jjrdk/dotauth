namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Endpoint mappings for the DotAuth UI surface.
/// </summary>
public static class DotAuthUiEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the built-in DotAuth UI endpoints and any feature-provided UI endpoint registrations.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapDotAuthUiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", CoreUiEndpointHandlers.GetHome).WithOrder(-1);
        endpoints.MapGet("/pwd", CoreUiEndpointHandlers.GetHome).WithOrder(-1);
        endpoints.MapGet("/home", CoreUiEndpointHandlers.GetHome).WithOrder(-1);
        endpoints.MapGet("/home/index", CoreUiEndpointHandlers.GetHome).WithOrder(-1);
        endpoints.MapGet("/pwd/home", CoreUiEndpointHandlers.GetHome).WithOrder(-1);
        endpoints.MapGet("/pwd/home/index", CoreUiEndpointHandlers.GetHome).WithOrder(-1);

        endpoints.MapGet("/error", CoreUiEndpointHandlers.GetError).WithOrder(-1);
        endpoints.MapGet("/error/400", CoreUiEndpointHandlers.GetError400).WithOrder(-1);
        endpoints.MapGet("/error/401", CoreUiEndpointHandlers.GetError401).WithOrder(-1);
        endpoints.MapGet("/error/404", CoreUiEndpointHandlers.GetError404).WithOrder(-1);
        endpoints.MapGet("/error/500", CoreUiEndpointHandlers.GetError500).WithOrder(-1);

        endpoints.MapGet("/form", CoreUiEndpointHandlers.GetForm).WithOrder(-1);
        endpoints.MapGet("/consent", CoreUiEndpointHandlers.GetConsent).WithOrder(-1);
        endpoints.MapPost("/consent/confirm", CoreUiEndpointHandlers.ConfirmConsent).WithOrder(-1);
        endpoints.MapPost("/consent/cancel", CoreUiEndpointHandlers.CancelConsent).WithOrder(-1);

        endpoints.MapGet("/authenticate", PasswordUiEndpointHandlers.GetAuthenticateIndex).WithOrder(-1);
        endpoints.MapPost("/authenticate/locallogin", PasswordUiEndpointHandlers.PostLocalLogin).WithOrder(-1);
        endpoints.MapGet("/authenticate/openid", PasswordUiEndpointHandlers.GetAuthenticateOpenId).WithOrder(-1);
        endpoints.MapPost("/authenticate/localloginopenid", PasswordUiEndpointHandlers.PostLocalLoginOpenId).WithOrder(-1);
        endpoints.MapGet("/authenticate/sendcode", PasswordUiEndpointHandlers.GetSendCode).WithOrder(-1);
        endpoints.MapPost("/authenticate/sendcode", PasswordUiEndpointHandlers.PostSendCode).WithOrder(-1);
        endpoints.MapGet("/authenticate/logout", PasswordUiEndpointHandlers.Logout).WithOrder(-1);
        endpoints.MapPost("/authenticate/externallogin", PasswordUiEndpointHandlers.ExternalLogin).WithOrder(-1);
        endpoints.MapGet("/authenticate/logincallback", PasswordUiEndpointHandlers.LoginCallback).WithOrder(-1);
        endpoints.MapPost("/authenticate/externalloginopenid", PasswordUiEndpointHandlers.ExternalLoginOpenId).WithOrder(-1);
        endpoints.MapGet("/authenticate/logincallbackopenid", PasswordUiEndpointHandlers.LoginCallbackOpenId).WithOrder(-1);

        endpoints.MapGet("/account", AccountUiEndpointHandlers.GetIndex).WithOrder(-1);
        endpoints.MapPost("/account", AccountUiEndpointHandlers.PostIndex).RequireAuthorization().WithOrder(-1);
        endpoints.MapGet("/account/accessdenied", AccountUiEndpointHandlers.GetAccessDenied).WithOrder(-1);

        endpoints.MapGet("/device", DeviceUiEndpointHandlers.Get).RequireAuthorization().WithOrder(-1);
        endpoints.MapPost("/device", DeviceUiEndpointHandlers.Approve).RequireAuthorization().WithOrder(-1);

        endpoints.MapUserUiEndpoints();

        endpoints.MapGet("/pwd/authenticate", PasswordUiEndpointHandlers.GetAuthenticateIndex).WithOrder(-1);
        endpoints.MapPost("/pwd/authenticate/locallogin", PasswordUiEndpointHandlers.PostLocalLogin).WithOrder(-1);
        endpoints.MapGet("/pwd/authenticate/openid", PasswordUiEndpointHandlers.GetAuthenticateOpenId).WithOrder(-1);
        endpoints.MapPost("/pwd/authenticate/localloginopenid", PasswordUiEndpointHandlers.PostLocalLoginOpenId).WithOrder(-1);
        endpoints.MapGet("/pwd/authenticate/sendcode", PasswordUiEndpointHandlers.GetSendCode).WithOrder(-1);
        endpoints.MapPost("/pwd/authenticate/sendcode", PasswordUiEndpointHandlers.PostSendCode).WithOrder(-1);
        endpoints.MapGet("/pwd/authenticate/logout", PasswordUiEndpointHandlers.Logout).WithOrder(-1);
        endpoints.MapPost("/pwd/authenticate/externallogin", PasswordUiEndpointHandlers.ExternalLogin).WithOrder(-1);
        endpoints.MapGet("/pwd/authenticate/logincallback", PasswordUiEndpointHandlers.LoginCallback).WithOrder(-1);
        endpoints.MapPost("/pwd/authenticate/externalloginopenid", PasswordUiEndpointHandlers.ExternalLoginOpenId).WithOrder(-1);
        endpoints.MapGet("/pwd/authenticate/logincallbackopenid", PasswordUiEndpointHandlers.LoginCallbackOpenId).WithOrder(-1);

        foreach (var registration in endpoints.ServiceProvider.GetServices<IDotAuthUiEndpointRegistration>())
        {
            registration.MapEndpoints(endpoints);
        }

        return endpoints;
    }
}


