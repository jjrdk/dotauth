namespace DotAuth.Extensions;

#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using DotAuth.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

/// <summary>
/// URL helper extensions for generating internal DotAuth links from Razor views.
/// </summary>
public static class UrlHelperExtensions
{
    public static string HomePath(this IUrlHelper url) => ContentPath(url, string.Empty);

    public static string UserPath(this IUrlHelper url) => ContentPath(url, "user");

    public static string UserConsentPath(this IUrlHelper url, string? id = null)
    {
        var path = ContentPath(url, "user/consent");
        return string.IsNullOrWhiteSpace(id) ? path : AddQuery(path, "id", id);
    }

    public static string UserLinkPath(this IUrlHelper url) => ContentPath(url, "user/link");

    public static string UserUnlinkPath(this IUrlHelper url, string id) => AddQuery(ContentPath(url, "user/unlink"), "id", id);

    public static string UserUpdateCredentialsPath(this IUrlHelper url) => ContentPath(url, "user/updatecredentials");

    public static string UserUpdateTwoFactorPath(this IUrlHelper url) => ContentPath(url, "user/updatetwofactor");

    public static string UserConfirmProfileLinkingPath(this IUrlHelper url) => ContentPath(url, "user/confirmprofilelinking");

    public static string PermissionRequestsPath(this IUrlHelper url) => ContentPath(url, UmaConstants.RouteValues.Permission);

    public static string PermissionApprovePath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{UmaConstants.RouteValues.Permission}/{Uri.EscapeDataString(id)}/approve");

    public static string AuthenticateRootPath(this IUrlHelper url) =>
        url.ActionContext.HttpContext.Request.Path.StartsWithSegments("/pwd")
            ? ContentPath(url, "pwd/authenticate")
            : ContentPath(url, "authenticate");

    public static string AuthenticateExternalLoginPath(this IUrlHelper url) => $"{url.AuthenticateRootPath()}/externallogin";

    public static string AuthenticateLocalLoginPath(this IUrlHelper url) => $"{url.AuthenticateRootPath()}/locallogin";

    public static string AuthenticateExternalLoginOpenIdPath(this IUrlHelper url, string code) =>
        AddQuery($"{url.AuthenticateRootPath()}/externalloginopenid", "code", code);

    public static string AuthenticateLocalLoginOpenIdPath(this IUrlHelper url) => $"{url.AuthenticateRootPath()}/localloginopenid";

    public static string AuthenticateSendCodePath(this IUrlHelper url) => $"{url.AuthenticateRootPath()}/sendcode";

    public static string AuthenticateLogoutPath(this IUrlHelper url) => $"{url.AuthenticateRootPath()}/logout";

    public static string ConsentConfirmPath(this IUrlHelper url, string code) => AddQuery(ContentPath(url, "consent/confirm"), "code", code);

    public static string ConsentCancelPath(this IUrlHelper url, string code) => AddQuery(ContentPath(url, "consent/cancel"), "code", code);

    public static string DeviceApprovePath(this IUrlHelper url) => ContentPath(url, CoreConstants.EndPoints.Device);

    public static string ClientCreatePath(this IUrlHelper url) => ContentPath(url, $"{CoreConstants.EndPoints.Clients}/create");

    public static string ClientPath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{CoreConstants.EndPoints.Clients}/{Uri.EscapeDataString(id)}");

    public static string ResourceSetPath(this IUrlHelper url, bool includeUiQuery = false)
    {
        var path = ContentPath(url, UmaConstants.RouteValues.ResourceSet);
        return includeUiQuery ? AddQuery(path, "ui", "1") : path;
    }

    public static string ResourceSetPolicyPath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{UmaConstants.RouteValues.ResourceSet}/{Uri.EscapeDataString(id)}/policy");

    public static string ScopePath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{CoreConstants.EndPoints.Scopes}/{Uri.EscapeDataString(id)}");

    public static string ScopeUpdatePath(this IUrlHelper url, string name) =>
        ContentPath(url, $"{CoreConstants.EndPoints.Scopes}/{Uri.EscapeDataString(name)}");

    public static string ScopesPath(this IUrlHelper url) => ContentPath(url, CoreConstants.EndPoints.Scopes);

    public static string ResourceOwnersPath(this IUrlHelper url) => ContentPath(url, CoreConstants.EndPoints.ResourceOwners);

    public static string ResourceOwnerPath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{CoreConstants.EndPoints.ResourceOwners}/{Uri.EscapeDataString(id)}");

    public static string ResourceOwnerUpdatePath(this IUrlHelper url, string id) =>
        ContentPath(url, $"{CoreConstants.EndPoints.ResourceOwners}/{Uri.EscapeDataString(id)}/update");

    public static string SmsAuthenticateLocalLoginPath(this IUrlHelper url) => ContentPath(url, "sms/authenticate/locallogin");

    public static string SmsAuthenticateLocalLoginOpenIdPath(this IUrlHelper url) => ContentPath(url, "sms/authenticate/localloginopenid");

    public static string SmsAuthenticateConfirmCodePath(this IUrlHelper url) => ContentPath(url, "sms/authenticate/confirmcode");

    private static string ContentPath(IUrlHelper url, string relativePath)
    {
        var appRelativePath = string.IsNullOrWhiteSpace(relativePath)
            ? "~/"
            : $"~/{relativePath.TrimStart('/')}";
        return url.Content(appRelativePath);
    }

    private static string AddQuery(string path, string name, string value) =>
        QueryHelpers.AddQueryString(path, new Dictionary<string, string?> { [name] = value });
}

#pragma warning restore CS1591



