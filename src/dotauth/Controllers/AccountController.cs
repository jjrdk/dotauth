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

namespace DotAuth.Controllers;

using System.Threading.Tasks;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines the account controller
/// </summary>
[Route("Account")]
public sealed class AccountController : BaseController
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly LinkGenerator _urlHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="authenticationService"></param>
    /// <param name="authenticationSchemeProvider"></param>
    /// <param name="urlHelper"></param>
    public AccountController(
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        LinkGenerator urlHelper)
        : base(authenticationService)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
        _urlHelper = urlHelper;
    }

    /// <summary>
    /// Handles the default request.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser is { Identity: { IsAuthenticated: true } })
        {
            return RedirectToAction("Index", "User");
        }

        return RedirectToAction("Index", "Authenticate"); //View();
    }

    /// <summary>
    /// Handles the AccessDenied request.
    /// </summary>
    /// <returns></returns>
    [HttpGet("AccessDenied")]
    public async Task AccessDenied()
    {
        _ = Request.Query.TryGetValue("ReturnUrl", out var returnUrl);

        var scheme = await _authenticationSchemeProvider.GetDefaultChallengeSchemeAsync().ConfigureAwait(false);
        var values = string.IsNullOrWhiteSpace(returnUrl) ? null : new { ReturnUrl = returnUrl };
        var redirectUrl =
            _urlHelper.GetUriByAction(HttpContext, "LoginCallback", "Authenticate", values, Request.Scheme);

        await _authenticationService.ChallengeAsync(
                Request.HttpContext,
                scheme!.Name,
                new AuthenticationProperties { RedirectUri = redirectUrl })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the update account request.
    /// </summary>
    /// <param name="updateResourceOwnerViewModel">The update view model.</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Index(UpdateResourceOwnerViewModel? updateResourceOwnerViewModel)
    {
        if (updateResourceOwnerViewModel == null)
        {
            return BadRequest();
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser is { Identity: { IsAuthenticated: true } })
        {
            return RedirectToAction("Index", "User");
        }

        return RedirectToAction("Index", "Authenticate");
    }
}
