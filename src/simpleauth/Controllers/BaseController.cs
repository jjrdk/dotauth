// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Controllers;

using System.Security.Claims;
using System.Threading.Tasks;
using Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Properties;
using SimpleAuth.Shared;

/// <summary>
/// Defines the abstract base controller.
/// </summary>
/// <seealso cref="Controller" />
public abstract class BaseController : Controller
{
    /// <summary>
    /// The authentication service
    /// </summary>
    protected readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="authenticationService">The authentication service.</param>
    protected BaseController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Sets the user.
    /// </summary>
    /// <returns></returns>
    protected async Task<ClaimsPrincipal?> SetUser()
    {
        var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, CookieNames.CookieName).ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return null;
        }

        var isAuthenticated = authenticatedUser.Identity is { IsAuthenticated: true };
        ViewBag.IsAuthenticated = isAuthenticated;
        ViewBag.Name = isAuthenticated ? authenticatedUser.GetName()! : Strings.Unknown;

        return authenticatedUser;

    }

    /// <summary>
    /// Handles the default error redirection request.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error status code.</param>
    /// <param name="title">The error title.</param>
    /// <returns></returns>
    protected IActionResult SetRedirection(string message, string? code = null, string? title = null)
    {
        return RedirectToAction("Index", "Error", new { code, title, message });
    }
}