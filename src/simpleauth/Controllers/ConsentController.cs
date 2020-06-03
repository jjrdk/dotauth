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

namespace SimpleAuth.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Events.Openid;
    using Shared.Requests;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Events;
    using SimpleAuth.Filters;
    using ViewModels;

    /// <summary>
    /// Defines the consent controller.
    /// </summary>
    /// <seealso cref="BaseController" />
    [Authorize("authenticated")]
    [ThrottleFilter]
    public class ConsentController : BaseController
    {
        private readonly DisplayConsentAction _displayConsent;
        private readonly ConfirmConsentAction _confirmConsent;
        private readonly IDataProtector _dataProtector;
        private readonly IClientStore _clientStore;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentController"/> class.
        /// </summary>
        /// <param name="scopeRepository">The scope repository.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="consentRepository">The consent repository.</param>
        /// <param name="resourceOwnerStore">The resource owner store.</param>
        /// <param name="dataProtectionProvider">The data protection provider.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="tokenStore">The token store.</param>
        /// <param name="jwksStore">The JWKS store.</param>
        /// <param name="authorizationCodeStore">The authorization code store.</param>
        /// <param name="authenticationService">The authentication service.</param>
        public ConsentController(
            IScopeRepository scopeRepository,
            IClientStore clientStore,
            IConsentRepository consentRepository,
            IResourceOwnerStore resourceOwnerStore,
            IDataProtectionProvider dataProtectionProvider,
            IEventPublisher eventPublisher,
            ITokenStore tokenStore,
            IJwksStore jwksStore,
            IAuthorizationCodeStore authorizationCodeStore,
            IAuthenticationService authenticationService)
            : base(authenticationService)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _clientStore = clientStore;
            _eventPublisher = eventPublisher;
            _displayConsent = new DisplayConsentAction(
                scopeRepository,
                clientStore,
                consentRepository,
                authorizationCodeStore,
                tokenStore,
                jwksStore,
                eventPublisher);
            _confirmConsent = new ConfirmConsentAction(
                authorizationCodeStore,
                tokenStore,
                consentRepository,
                clientStore,
                scopeRepository,
                resourceOwnerStore,
                jwksStore,
                eventPublisher);
        }

        /// <summary>
        /// Get the default page.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(string code, CancellationToken cancellationToken)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = await _clientStore.GetById(request.client_id, cancellationToken).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _displayConsent.Execute(request.ToParameter(), authenticatedUser, issuerName, cancellationToken)
                .ConfigureAwait(false);

            var result = actionResult.EndpointResult.CreateRedirectionFromActionResult(request);
            if (result != null)
            {
                return result;
            }

            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.ClientName,
                AllowedScopeDescriptions =
                    actionResult.Scopes == null
                        ? new List<string>()
                        : actionResult.Scopes.Select(s => s.Description).ToList(),
                AllowedIndividualClaims = actionResult.AllowedClaims ?? new List<string>(),
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
        public async Task<IActionResult> Confirm(string code, CancellationToken cancellationToken)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var parameter = request.ToParameter();
            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, CookieNames.CookieName)
                .ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _confirmConsent.Execute(parameter, authenticatedUser, issuerName, cancellationToken)
                .ConfigureAwait(false);
            LogConsentAccepted(authenticatedUser.GetSubject(), request.client_id, request.scope);
            return actionResult.CreateRedirectionFromActionResult(request);
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted &amp; signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public Task<IActionResult> Cancel(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            LogConsentRejected(request.client_id, request.scope);
            var result = Redirect(request.redirect_uri.AbsoluteUri);

            return Task.FromResult<IActionResult>(result);
        }

        private void LogConsentAccepted(string subject, string clientId, string scope)
        {
            _eventPublisher.Publish(new ConsentAccepted(Id.Create(), subject, clientId, scope, DateTimeOffset.UtcNow));
        }

        private void LogConsentRejected(string clientId, string scope)
        {
            _eventPublisher.Publish(new ConsentRejected(Id.Create(), clientId, scope.Trim().Split(' '), DateTimeOffset.UtcNow));
        }
    }
}
