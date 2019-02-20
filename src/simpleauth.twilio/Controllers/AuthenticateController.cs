﻿namespace SimpleAuth.Twilio.Controllers
{
    using Actions;
    using Exceptions;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using SimpleAuth;
    using SimpleAuth.Controllers;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.ViewModels;
    using SimpleAuth.WebSite.Authenticate;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Logging;
    using ViewModels;
    using WebSite.User.Actions;

    [Area(SmsConstants.Amr)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly GetUserOperation _getUserOperation;
        private readonly SmsAuthenticationOperation _smsAuthenticationOperation;
        private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly ValidateConfirmationCodeAction _validateConfirmationCode;
        private readonly AuthenticateHelper _authenticateHelper;

        public AuthenticateController(
            ITwilioClient twilioClient,
            IDataProtectionProvider dataProtectionProvider,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventPublisher eventPublisher,
            IAuthorizationCodeStore authorizationCodeStore,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ISubjectBuilder subjectBuilder,
            IConsentRepository consentRepository,
            IScopeRepository scopeRepository,
            ITokenStore tokenStore,
            IResourceOwnerRepository resourceOwnerRepository,
            IConfirmationCodeStore confirmationCodeStore,
            IClientStore clientStore,
            IEnumerable<IAccountFilter> accountFilters,
            RuntimeSettings runtimeSettings,
            SmsAuthenticationOptions smsOptions)
            : base(
                dataProtectionProvider,
                urlHelperFactory,
                actionContextAccessor,
                eventPublisher,
                authenticationService,
                authenticationSchemeProvider,
                twoFactorAuthenticationHandler,
                authorizationCodeStore,
                subjectBuilder,
                consentRepository,
                scopeRepository,
                tokenStore,
                resourceOwnerRepository,
                confirmationCodeStore,
                clientStore,
                accountFilters,
                runtimeSettings)
        {
            _eventPublisher = eventPublisher;
            _getUserOperation = new GetUserOperation(resourceOwnerRepository);
            var generateSms = new GenerateAndSendSmsCodeOperation(twilioClient, confirmationCodeStore, smsOptions);
            _smsAuthenticationOperation = new SmsAuthenticationOperation(
                twilioClient,
                confirmationCodeStore,
                resourceOwnerRepository,
                subjectBuilder,
                accountFilters,
                eventPublisher,
                smsOptions);
            _validateConfirmationCode = new ValidateConfirmationCodeAction(confirmationCodeStore);
            _authenticateHelper = new AuthenticateHelper(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                consentRepository,
                clientStore,
                eventPublisher);
            _generateAndSendSmsCodeOperation = generateSms;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        public async Task<IActionResult> LocalLogin(
            SmsAuthenticationViewModel localAuthenticationViewModel,
            CancellationToken cancellationToken)
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            if (localAuthenticationViewModel == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationViewModel));
            }

            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation
                        .Execute(localAuthenticationViewModel.PhoneNumber, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _eventPublisher.Publish(new ExceptionMessage(Id.Create(), ex, DateTime.UtcNow))
                        .ConfigureAwait(false);
                    // _openIdEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    resourceOwner.Claims = resourceOwner.Claims.Add(
                        new Claim(
                            ClaimTypes.AuthenticationInstant,
                            DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                            ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(resourceOwner.Claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode");
                    }
                    catch (Exception ex)
                    {
                        await _eventPublisher.Publish(new ExceptionMessage(Id.Create(), ex, DateTime.UtcNow))
                            .ConfigureAwait(false);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmCode(string code)
        {
            var user = await SetUser().ConfigureAwait(false);
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, CookieNames.PasswordLessCookieName)
                .ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    "SMS authentication cannot be performed");
            }

            return View(new ConfirmCodeViewModel { Code = code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCode(
            ConfirmCodeViewModel confirmCodeViewModel,
            CancellationToken cancellationToken)
        {
            if (confirmCodeViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmCodeViewModel));
            }

            var user = await SetUser().ConfigureAwait(false);
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }

            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, CookieNames.PasswordLessCookieName)
                .ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    "SMS authentication cannot be performed");
            }

            var authenticatedUserClaims = authenticatedUser.Claims.ToArray();
            var subject = authenticatedUserClaims
                .First(c => c.Type == OpenIdClaimTypes.Subject)
                .Value;
            var phoneNumber = authenticatedUserClaims.First(
                c => c.Type == OpenIdClaimTypes.PhoneNumber);
            if (confirmCodeViewModel.Action == "resend") // Resend the confirmation code.
            {
                var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value).ConfigureAwait(false);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            if (!await _validateConfirmationCode.Execute(confirmCodeViewModel.ConfirmationCode).ConfigureAwait(false)
            ) // Check the confirmation code.
            {
                ModelState.AddModelError("message_error", "Confirmation code is not valid");
                return View("ConfirmCode", confirmCodeViewModel);
            }

            await _authenticationService.SignOutAsync(
                    HttpContext,
                    CookieNames.PasswordLessCookieName,
                    new AuthenticationProperties())
                .ConfigureAwait(false);
            var resourceOwner = await _getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication)) // Execute TWO Factor authentication
            {
                try
                {
                    await SetTwoFactorCookie(authenticatedUserClaims).ConfigureAwait(false);
                    await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value).ConfigureAwait(false);
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                    return View("ConfirmCode", confirmCodeViewModel);
                }
            }

            //_openIdEventSource.AuthenticateResourceOwner(subject);
            if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)) // Execute OPENID workflow
            {
                var request = _dataProtector.Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
                await SetLocalCookie(authenticatedUserClaims, request.session_id).ConfigureAwait(false);
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateHelper.ProcessRedirection(
                        request.ToParameter(),
                        confirmCodeViewModel.Code,
                        subject,
                        authenticatedUserClaims,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
                var result = this.CreateRedirectionFromActionResult(actionResult, request);
                if (result != null)
                {
                    await LogAuthenticateUser(actionResult, request.aggregate_id).ConfigureAwait(false);
                    return result;
                }
            }

            await SetLocalCookie(authenticatedUserClaims, Id.Create())
                .ConfigureAwait(false); // Authenticate the resource owner
            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        public async Task<IActionResult> LocalLoginOpenId(
            SmsOpenIdLocalAuthenticationViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                throw new ArgumentNullException(nameof(viewModel.Code));
            }

            await SetUser().ConfigureAwait(false);
            // 1. Decrypt the request
            var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);
            // 2. Retrieve the default language
            var uiLocales = string.IsNullOrWhiteSpace(request.ui_locales) ? DefaultLanguage : request.ui_locales;
            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(viewModel.PhoneNumber, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _eventPublisher.Publish(new ExceptionMessage(Id.Create(), ex, DateTime.UtcNow))
                        .ConfigureAwait(false);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    resourceOwner.Claims = resourceOwner.Claims.Add(
                        new Claim(
                            ClaimTypes.AuthenticationInstant,
                            DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                            ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(resourceOwner.Claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode", new { code = viewModel.Code });
                    }
                    catch (Exception ex)
                    {
                        await _eventPublisher.Publish(new ExceptionMessage(Id.Create(), ex, DateTime.UtcNow))
                            .ConfigureAwait(false);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("OpenId", viewModel);
        }

        private async Task SetPasswordLessCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, CookieNames.PasswordLessCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(
                    HttpContext,
                    CookieNames.PasswordLessCookieName,
                    principal,
                    new AuthenticationProperties { ExpiresUtc = DateTime.UtcNow.AddMinutes(20), IsPersistent = false })
                .ConfigureAwait(false);
        }
    }
}
