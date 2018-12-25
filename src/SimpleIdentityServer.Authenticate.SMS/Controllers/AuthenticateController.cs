namespace SimpleIdentityServer.Authenticate.SMS.Controllers
{
    using Host.Controllers;
    using Host.ViewModels;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Actions;
    using ViewModels;
    using Host.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SimpleAuth;
    using SimpleAuth.Api.Profile;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Extensions;
    using SimpleAuth.Logging;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Translation;
    using SimpleAuth.WebSite.Authenticate;
    using SimpleAuth.WebSite.Authenticate.Common;
    using SimpleAuth.WebSite.User.Actions;

    [Area(SmsConstants.AMR)]
    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IGetUserOperation _getUserOperation;
        private readonly ISmsAuthenticationOperation _smsAuthenticationOperation;
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

        public AuthenticateController(
            IAuthenticateActions authenticateActions,
            IProfileActions profileActions,
            IDataProtectionProvider dataProtectionProvider,
            ITranslationManager translationManager,
            IOpenIdEventSource simpleIdentityServerEventSource,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IAddUserOperation userActions,
            IGetUserOperation getUserOperation,
            IUpdateUserClaimsOperation updateUserClaimsOperation,
            OAuthConfigurationOptions configurationService,
            IAuthenticateHelper authenticateHelper,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ISmsAuthenticationOperation smsAuthenticationOperation,
            IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation,
            ISubjectBuilder subjectBuilder,
            SmsAuthenticationOptions basicAuthenticateOptions)
            : base(authenticateActions,
                profileActions,
                dataProtectionProvider,
                translationManager,
                simpleIdentityServerEventSource,
                urlHelperFactory,
                actionContextAccessor,
                eventPublisher,
                authenticationService,
                authenticationSchemeProvider,
                userActions,
                getUserOperation,
                updateUserClaimsOperation,
                configurationService,
                authenticateHelper,
                twoFactorAuthenticationHandler,
                subjectBuilder,
                basicAuthenticateOptions)
        {
            _getUserOperation = getUserOperation;
            _smsAuthenticationOperation = smsAuthenticationOperation;
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View(viewModel);
            }

            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        public async Task<IActionResult> LocalLogin(LocalAuthenticationViewModel localAuthenticationViewModel)
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
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
                    resourceOwner = await _smsAuthenticationOperation.Execute(localAuthenticationViewModel.PhoneNumber)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode");
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmCode(string code)
        {
            var user = await SetUser().ConfigureAwait(false);
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, Host.HostConstants.CookieNames.PasswordLessCookieName)
                .ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    "SMS authentication cannot be performed");
            }

            await TranslateView(DefaultLanguage).ConfigureAwait(false);
            return View(new ConfirmCodeViewModel
            {
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCode(ConfirmCodeViewModel confirmCodeViewModel)
        {
            if (confirmCodeViewModel == null)
            {
                throw new ArgumentNullException(nameof(confirmCodeViewModel));
            }

            var user = await SetUser().ConfigureAwait(false);
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new { area = "UserManagement" });
            }

            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, Host.HostConstants.CookieNames.PasswordLessCookieName)
                .ConfigureAwait(false);
            if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                throw new IdentityServerException(ErrorCodes.UnhandledExceptionCode,
                    "SMS authentication cannot be performed");
            }

            var subject = authenticatedUser.Claims
                .First(c => c.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject)
                .Value;
            var phoneNumber = authenticatedUser.Claims.First(c =>
                c.Type == JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber);
            if (confirmCodeViewModel.Action == "resend") // Resend the confirmation code.
            {
                var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value).ConfigureAwait(false);
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            if (!await _authenticateActions.ValidateCode(confirmCodeViewModel.ConfirmationCode).ConfigureAwait(false)
            ) // Check the confirmation code.
            {
                ModelState.AddModelError("message_error", "Confirmation code is not valid");
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                return View("ConfirmCode", confirmCodeViewModel);
            }

            await _authenticationService.SignOutAsync(HttpContext,
                    Host.HostConstants.CookieNames.PasswordLessCookieName,
                    new AuthenticationProperties())
                .ConfigureAwait(false);
            var resourceOwner = await _getUserOperation.Execute(authenticatedUser).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication)) // Execute TWO Factor authentication
            {
                try
                {
                    await SetTwoFactorCookie(authenticatedUser.Claims).ConfigureAwait(false);
                    var code = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value).ConfigureAwait(false);
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                    await TranslateView(DefaultLanguage).ConfigureAwait(false);
                    return View("ConfirmCode", confirmCodeViewModel);
                }
            }

            _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
            if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)) // Execute OPENID workflow
            {
                var request = _dataProtector.Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
                await SetLocalCookie(authenticatedUser.Claims, request.SessionId).ConfigureAwait(false);
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateHelper.ProcessRedirection(request.ToParameter(),
                        confirmCodeViewModel.Code,
                        subject,
                        authenticatedUser.Claims.ToList(),
                        issuerName)
                    .ConfigureAwait(false);
                var result = this.CreateRedirectionFromActionResult(actionResult, request);
                if (result != null)
                {
                    LogAuthenticateUser(actionResult, request.ProcessId);
                    return result;
                }
            }

            await SetLocalCookie(authenticatedUser.Claims, Guid.NewGuid().ToString())
                .ConfigureAwait(false); // Authenticate the resource owner
            return RedirectToAction("Index", "User", new { area = "UserManagement" });
        }

        [HttpPost]
        public async Task<IActionResult> LocalLoginOpenId(OpenidLocalAuthenticationViewModel viewModel)
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
            var uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;
            if (ModelState.IsValid)
            {
                ResourceOwner resourceOwner = null;
                try
                {
                    resourceOwner = await _smsAuthenticationOperation.Execute(viewModel.PhoneNumber)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _simpleIdentityServerEventSource.Failure(ex.Message);
                    ModelState.AddModelError("message_error", ex.Message);
                }

                if (resourceOwner != null)
                {
                    var claims = resourceOwner.Claims;
                    claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                        DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer));
                    await SetPasswordLessCookie(claims).ConfigureAwait(false);
                    try
                    {
                        return RedirectToAction("ConfirmCode", new { code = viewModel.Code });
                    }
                    catch (Exception ex)
                    {
                        _simpleIdentityServerEventSource.Failure(ex.Message);
                        ModelState.AddModelError("message_error", "TWILIO account is not valid");
                    }
                }
            }

            await TranslateView(uiLocales).ConfigureAwait(false);
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("OpenId", viewModel);
        }

        private async Task SetPasswordLessCookie(IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, Host.HostConstants.CookieNames.PasswordLessCookieName);
            var principal = new ClaimsPrincipal(identity);
            await _authenticationService.SignInAsync(HttpContext,
                    Host.HostConstants.CookieNames.PasswordLessCookieName,
                    principal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = false
                    })
                .ConfigureAwait(false);
        }
    }
}
