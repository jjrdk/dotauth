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
    using Results;
    using Shared;
    using Shared.Events.Openid;
    using Shared.Models;
    using Shared.Requests;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ViewModels;

    [Authorize("Connected")]
    public class ConsentController : BaseController
    {
        private readonly DisplayConsentAction _displayConsent;
        private readonly ConfirmConsentAction _confirmConsent;
        private readonly IDataProtector _dataProtector;
        private readonly IEventPublisher _eventPublisher;

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

        public async Task<IActionResult> Index(string code, CancellationToken cancellationToken)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = new Client();
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _displayConsent.Execute(request.ToParameter(), authenticatedUser, issuerName, cancellationToken)
                .ConfigureAwait(false);

            var result = this.CreateRedirectionFromActionResult(actionResult.EndpointResult, request);
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
                //LogoUri = client?.LogoUri?.AbsoluteUri,
                PolicyUri = client.PolicyUri?.AbsoluteUri,
                TosUri = client.TosUri?.AbsoluteUri,
                Code = code
            };
            return View("Index", viewModel);
        }

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
            LogConsentAccepted(actionResult, parameter.ProcessId);
            return this.CreateRedirectionFromActionResult(actionResult, request);
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
            LogConsentRejected(request.aggregate_id);
            var result = Redirect(request.redirect_uri.AbsoluteUri);

            return Task.FromResult<IActionResult>(result);
        }

        private void LogConsentAccepted(EndpointResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ConsentAccepted(Id.Create(), processId, act, DateTime.UtcNow));
        }

        private void LogConsentRejected(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ConsentRejected(Id.Create(), processId, DateTime.UtcNow));
        }
    }
}
