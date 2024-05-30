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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Shared;
using DotAuth.Shared.Events.Openid;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.ViewModels;
using DotAuth.WebSite.Consent.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the consent controller.
/// </summary>
/// <seealso cref="BaseController" />
[Authorize("authenticated")]
[ThrottleFilter]
public sealed class ConsentController : BaseController
{
    private readonly DisplayConsentAction _displayConsent;
    private readonly ConfirmConsentAction _confirmConsent;
    private readonly IDataProtector _dataProtector;
    private readonly IClientStore _clientStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ConsentController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentController"/> class.
    /// </summary>
    /// <param name="scopeRepository">The scope repository.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="consentRepository">The consent repository.</param>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="jwksStore">The JWKS store.</param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="logger">The controller logger.</param>
    public ConsentController(
        IScopeRepository scopeRepository,
        IClientStore clientStore,
        IConsentRepository consentRepository,
        IDataProtectionProvider dataProtectionProvider,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IAuthorizationCodeStore authorizationCodeStore,
        IAuthenticationService authenticationService,
        ILogger<ConsentController> logger)
        : base(authenticationService)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Request");
        _clientStore = clientStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _displayConsent = new DisplayConsentAction(
            scopeRepository,
            clientStore,
            consentRepository,
            authorizationCodeStore,
            tokenStore,
            jwksStore,
            eventPublisher,
            logger);
        _confirmConsent = new ConfirmConsentAction(
            authorizationCodeStore,
            tokenStore,
            consentRepository,
            clientStore,
            scopeRepository,
            jwksStore,
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Get the default page.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index(string code, CancellationToken cancellationToken)
    {
        var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
        if (request.client_id == null)
        {
            return BadRequest();
        }
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await _displayConsent.Execute(
                request.ToParameter(),
                authenticatedUser ?? new ClaimsPrincipal(),
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);

        var result = actionResult.EndpointResult.CreateRedirectionFromActionResult(request, _logger);
        if (result != null)
        {
            return result;
        }

        var client = await _clientStore.GetById(request.client_id, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return BadRequest();
        }
        var viewModel = new ConsentViewModel
        {
            ClientDisplayName = client.ClientName,
            AllowedScopeDescriptions =
                actionResult?.Scopes == null
                    ? []
                    : actionResult.Scopes.Select(s => s.Description).ToList(),
            AllowedIndividualClaims = actionResult?.AllowedClaims ?? new List<string>(),
            LogoUri = client.LogoUri?.AbsoluteUri,
            PolicyUri = client.PolicyUri?.AbsoluteUri,
            TosUri = client.TosUri?.AbsoluteUri,
            Code = code
        };
        return Ok(viewModel);
    }

    /// <summary>
    /// Confirms the specified code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Confirm(string code, CancellationToken cancellationToken)
    {
        var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
        var parameter = request.ToParameter();
        var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, CookieNames.CookieName)
            .ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Unauthorized();
        }
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await _confirmConsent
            .Execute(parameter, authenticatedUser, issuerName, cancellationToken)
            .ConfigureAwait(false);

        var subject = authenticatedUser.GetSubject();

        await _eventPublisher.Publish(
                new ConsentAccepted(
                    Id.Create(),
                    subject!,
                    request.client_id!,
                    request.scope!,
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        return actionResult.CreateRedirectionFromActionResult(request, _logger)!;
    }

    /// <summary>
    /// Action executed when the user refuse the consent.
    /// It redirects to the callback without passing the authorization code in parameter.
    /// </summary>
    /// <param name="code">Encrypted &amp; signed authorization request</param>
    /// <returns>Redirect to the callback url.</returns>
    [HttpPost]
    public async Task<IActionResult> Cancel(string code)
    {
        var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
        if (request?.redirect_uri == null)
        {
            return BadRequest();
        }

        await _eventPublisher.Publish(
                new ConsentRejected(
                    Id.Create(),
                    request.client_id ?? string.Empty,
                    request.scope == null ? [] : request.scope.Trim().Split(' '),
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        return Redirect(request.redirect_uri.AbsoluteUri);
    }
}