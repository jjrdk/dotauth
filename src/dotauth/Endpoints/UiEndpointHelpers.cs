namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Results;
using DotAuth.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

internal static class UiEndpointHelpers
{
    internal static bool WantsHtml(HttpRequest request)
    {
        var acceptHeaders = request.GetTypedHeaders().Accept;
        if (acceptHeaders.Count == 0)
        {
            return false;
        }

        return acceptHeaders.Any(header =>
            header.MediaType.HasValue
            && (string.Equals(header.MediaType.Value, "text/html", StringComparison.OrdinalIgnoreCase)
                || string.Equals(header.MediaType.Value, "text/xhtml", StringComparison.OrdinalIgnoreCase)
                || string.Equals(header.MediaType.Value, "application/xhtml", StringComparison.OrdinalIgnoreCase)
                || string.Equals(header.MediaType.Value, "application/xhtml+xml", StringComparison.OrdinalIgnoreCase)));
    }

    internal static IResult ViewOrJson(HttpContext httpContext, string viewPath, object? model, int statusCode = StatusCodes.Status200OK, IReadOnlyDictionary<string, object?>? viewData = null, ModelStateDictionary? modelState = null)
    {
        return WantsHtml(httpContext.Request)
            ? new RazorViewResult(viewPath, model, statusCode, viewData, modelState)
            : Results.Json(model, statusCode: statusCode);
    }

    internal static IResult RedirectToError(string message, string? code = null, string? title = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["code"] = code,
            ["title"] = title,
            ["message"] = message
        };

        return Results.Redirect(QueryHelpers.AddQueryString("/error", query!));
    }

    internal static IResult ToRedirectResult(HttpContext httpContext, ActionResult actionResult)
    {
        return actionResult switch
        {
            RedirectResult redirectResult => Results.Redirect(redirectResult.Url!),
            RedirectToRouteResult redirectToRouteResult => Results.Redirect(
                httpContext.RequestServices.GetRequiredService<LinkGenerator>().GetPathByRouteValues(
                    httpContext,
                    routeName: null,
                    values: redirectToRouteResult.RouteValues) ?? "/"),
            _ => Results.BadRequest()
        };
    }

    internal static async Task<ClaimsPrincipal?> SetUserAsync(HttpContext httpContext, IAuthenticationService authenticationService, string scheme = CookieNames.CookieName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(authenticationService);

        return await authenticationService.GetAuthenticatedUser(httpContext, scheme).ConfigureAwait(false);
    }

    internal static RouteValueDictionary BuildRouteValues(params (string key, object? value)[] values)
    {
        var result = new RouteValueDictionary();
        foreach (var (key, value) in values)
        {
            if (value != null)
            {
                result[key] = value;
            }
        }

        return result;
    }

    internal static string BuildActionPath(HttpContext httpContext, string controller, string action, string? area = null, RouteValueDictionary? values = null)
    {
        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
        var routeValues = values ?? new RouteValueDictionary();
        routeValues["controller"] = controller;
        routeValues["action"] = action;
        if (area != null)
        {
            routeValues["area"] = area;
        }

        return linkGenerator.GetPathByRouteValues(httpContext, routeName: null, values: routeValues) ?? "/";
    }

    internal static async Task SetLocalCookieAsync(HttpContext httpContext, IAuthenticationService authenticationService, RuntimeSettings runtimeSettings, Claim[] claims, string sessionId)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(runtimeSettings.RptLifeTime);
        httpContext.Response.Cookies.Append(
            CoreConstants.SessionId,
            sessionId,
            new CookieOptions
            {
                HttpOnly = runtimeSettings.AllowHttp,
                Secure = !runtimeSettings.AllowHttp,
                Expires = expires,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
            });
        var identity = new ClaimsIdentity(claims, CookieNames.CookieName);
        var principal = new ClaimsPrincipal(identity);
        await authenticationService.SignInAsync(
                httpContext,
                CookieNames.CookieName,
                principal,
                new AuthenticationProperties
                {
                    IssuedUtc = now,
                    ExpiresUtc = expires,
                    AllowRefresh = false,
                    IsPersistent = false
                })
            .ConfigureAwait(false);
    }

    internal static async Task SetTwoFactorCookieAsync(HttpContext httpContext, IAuthenticationService authenticationService, Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, CookieNames.TwoFactorCookieName);
        var principal = new ClaimsPrincipal(identity);
        await authenticationService.SignInAsync(
                httpContext,
                CookieNames.TwoFactorCookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                    IsPersistent = false
                })
            .ConfigureAwait(false);
    }

    internal static string GetRedirectPathForEndpoint(DotAuthEndPoints endpoint, string? amr = null)
    {
        var partialUri = endpoint switch
        {
            DotAuthEndPoints.AuthenticateIndex => "/Authenticate/OpenId",
            DotAuthEndPoints.ConsentIndex => "/Consent",
            DotAuthEndPoints.FormIndex => "/Form",
            DotAuthEndPoints.SendCode => "/Authenticate/SendCode",
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null)
        };

        if (!string.IsNullOrWhiteSpace(amr)
            && endpoint != DotAuthEndPoints.ConsentIndex
            && endpoint != DotAuthEndPoints.FormIndex)
        {
            partialUri = $"/{amr}{partialUri}";
        }

        return partialUri;
    }

    internal static ErrorDetails CreateErrorDetails(string code, string message, HttpStatusCode statusCode)
    {
        return new ErrorDetails { Title = code, Detail = message, Status = statusCode };
    }

    internal static Dictionary<string, object?> CreateViewData(params (string key, object? value)[] entries)
    {
        var result = new Dictionary<string, object?>();
        foreach (var (key, value) in entries)
        {
            result[key] = value;
        }

        return result;
    }
}


