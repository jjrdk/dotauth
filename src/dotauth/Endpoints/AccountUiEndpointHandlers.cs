namespace DotAuth.Endpoints;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

internal static class AccountUiEndpointHandlers
{
    internal static async Task<IResult> GetIndex(
        HttpContext httpContext,
        IAuthenticationService authenticationService)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        return authenticatedUser is { Identity: { IsAuthenticated: true } }
            ? Results.Redirect("/user")
            : Results.Redirect("/authenticate");
    }

    internal static async Task GetAccessDenied(
        HttpContext httpContext,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IAuthenticationService authenticationService)
    {
        httpContext.Request.Query.TryGetValue("ReturnUrl", out var returnUrl);

        var scheme = await authenticationSchemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false);
        var redirectUri = BuildAuthenticateCallbackUri(httpContext, returnUrl.ToString());
        await authenticationService.ChallengeAsync(
                httpContext,
                scheme!.Name,
                new AuthenticationProperties { RedirectUri = redirectUri })
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> PostIndex(
        HttpContext httpContext,
        IAuthenticationService authenticationService)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest();
        }

        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        return authenticatedUser is { Identity: { IsAuthenticated: true } }
            ? Results.Redirect("/user")
            : Results.Redirect("/authenticate");
    }

    private static string BuildAuthenticateCallbackUri(HttpContext httpContext, string? returnUrl)
    {
        var query = string.IsNullOrWhiteSpace(returnUrl)
            ? null
            : new Dictionary<string, string?> { ["ReturnUrl"] = returnUrl };
        var path = query == null
            ? "/authenticate/logincallback"
            : QueryHelpers.AddQueryString("/authenticate/logincallback", query);
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}{path}";
    }
}


