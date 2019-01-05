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

namespace SimpleAuth.Server.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Results;
    using Server;
    using Shared;
    using Shared.Events.Openid;
    using Shared.Models;
    using Shared.Requests;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Translation;
    using ViewModels;
    using WebSite.Consent;

    [Authorize("Connected")]
    public class ConsentController : BaseController
    {
        private readonly IConsentActions _consentActions;
        private readonly IDataProtector _dataProtector;
        private readonly ITranslationManager _translationManager;
        private readonly IEventPublisher _eventPublisher;

        public ConsentController(
            IConsentActions consentActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService) : base(authenticationService)
        {
            _consentActions = consentActions;
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _translationManager = translationManager;
            _eventPublisher = eventPublisher;
        }

        public async Task<IActionResult> Index(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var client = new Client();
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _consentActions.DisplayConsent(request.ToParameter(),
                    authenticatedUser,
                    issuerName)
                .ConfigureAwait(false);

            var result = this.CreateRedirectionFromActionResult(actionResult.EndpointResult, request);
            if (result != null)
            {
                return result;
            }

            await TranslateConsentScreen(request.UiLocales).ConfigureAwait(false);
            var viewModel = new ConsentViewModel
            {
                ClientDisplayName = client.ClientName,
                AllowedScopeDescriptions = actionResult.Scopes == null
                    ? new List<string>()
                    : actionResult.Scopes.Select(s => s.Description).ToList(),
                AllowedIndividualClaims = actionResult.AllowedClaims ?? new List<string>(),
                //LogoUri = client?.LogoUri?.AbsoluteUri,
                PolicyUri = client?.PolicyUri?.AbsoluteUri,
                TosUri = client?.TosUri?.AbsoluteUri,
                Code = code
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Confirm(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            var parameter = request.ToParameter();
            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName)
                .ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _consentActions.ConfirmConsent(parameter,
                    authenticatedUser,
                    issuerName)
                .ConfigureAwait(false);
            LogConsentAccepted(actionResult, parameter.ProcessId);
            return this.CreateRedirectionFromActionResult(actionResult,
                request);
        }

        /// <summary>
        /// Action executed when the user refuse the consent.
        /// It redirects to the callback without passing the authorization code in parameter.
        /// </summary>
        /// <param name="code">Encrypted & signed authorization request</param>
        /// <returns>Redirect to the callback url.</returns>
        public Task<IActionResult> Cancel(string code)
        {
            var request = _dataProtector.Unprotect<AuthorizationRequest>(code);
            LogConsentRejected(request.ProcessId);
            var result = Redirect(request.RedirectUri.AbsoluteUri);

            return Task.FromResult<IActionResult>(result);
        }

        private async Task TranslateConsentScreen(string uiLocales)
        {
            // Retrieve the translation and store them in a ViewBag
            var translations = await _translationManager.GetTranslationsAsync(uiLocales,
                    new List<string>
                    {
                        CoreConstants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                        CoreConstants.StandardTranslationCodes.IndividualClaimsCode,
                        CoreConstants.StandardTranslationCodes.ScopesCode,
                        CoreConstants.StandardTranslationCodes.CancelCode,
                        CoreConstants.StandardTranslationCodes.ConfirmCode,
                        CoreConstants.StandardTranslationCodes.LinkToThePolicy,
                        CoreConstants.StandardTranslationCodes.Tos
                    })
                .ConfigureAwait(false);
            ViewBag.Translations = translations;
        }

        private void LogConsentAccepted(EndpointResult act, string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(
                new ConsentAccepted(
                    Guid.NewGuid().ToString(),
                    processId,
                    act,
                    10));
        }

        private void LogConsentRejected(string processId)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                return;
            }

            _eventPublisher.Publish(new ConsentRejected(Guid.NewGuid().ToString(), processId, 10));
        }
    }
}
