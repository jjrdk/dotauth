namespace DotAuth.Extensions;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines the authenticate service extensions.
/// </summary>
internal static class AuthenticateServiceExtensions
{
    /// <summary>
    /// Gets the authenticated user.
    /// </summary>
    /// <param name="authenticateService">The authenticate service.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="scheme">The scheme.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// authenticateService
    /// or
    /// controller
    /// or
    /// </exception>
    public static async Task<ClaimsPrincipal?> GetAuthenticatedUser(this IAuthenticationService authenticateService, ControllerBase controller, string? scheme = null)
    {
        var authResult = await authenticateService.AuthenticateAsync(controller.HttpContext, scheme).ConfigureAwait(false);
        return authResult.Principal;
    }
}