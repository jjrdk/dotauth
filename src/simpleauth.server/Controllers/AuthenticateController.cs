namespace SimpleAuth.Server.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Logging;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Parameters;
    using Server;
    using Shared;
    using Shared.Requests;
    using SimpleAuth;
    using SimpleAuth.Api.Profile;
    using SimpleAuth.Extensions;
    using SimpleAuth.Services;
    using Translation;
    using ViewModels;
    using WebSite.Authenticate;
    using WebSite.Authenticate.Common;
    using WebSite.User.Actions;

    public class AuthenticateController : BaseAuthenticateController
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

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
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
            ISubjectBuilder subjectBuilder,
            BasicAuthenticateOptions basicAuthenticateOptions)
            : base(
                authenticateActions,
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
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
        }

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

            return RedirectToAction("Index", "User", new {area = "UserManagement"});
        }

        [HttpPost]
        public async Task<IActionResult> LocalLogin(LocalAuthenticationViewModel authorizeViewModel)
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            if (authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User", new {area = "UserManagement"});
            }

            if (authorizeViewModel == null)
            {
                throw new ArgumentNullException(nameof(authorizeViewModel));
            }

            if (!ModelState.IsValid)
            {
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View("Index", viewModel);
            }

            try
            {
                var resourceOwner = await _resourceOwnerAuthenticateHelper
                    .Authenticate(authorizeViewModel.Login, authorizeViewModel.Password)
                    .ConfigureAwait(false);
                if (resourceOwner == null)
                {
                    throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
                }

                var claims = resourceOwner.Claims;
                claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
                var subject = claims.First(c => c.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject)
                    .Value;
                if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
                {
                    await SetLocalCookie(claims, Guid.NewGuid().ToString()).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
                    return RedirectToAction("Index", "User", new {area = "UserManagement"});
                }

                // 2.1 Store temporary information in cookie
                await SetTwoFactorCookie(claims).ConfigureAwait(false);
                // 2.2. Send confirmation code
                try
                {
                    var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.GetConfirmationCode(code);
                    return RedirectToAction("SendCode");
                }
                catch (ClaimRequiredException)
                {
                    return RedirectToAction("SendCode");
                }
                catch (Exception)
                {
                    throw new Exception("Two factor authenticator is not properly configured");
                }
            }
            catch (Exception exception)
            {
                _simpleIdentityServerEventSource.Failure(exception.Message);
                await TranslateView(DefaultLanguage).ConfigureAwait(false);
                ModelState.AddModelError("invalid_credentials", exception.Message);
                var viewModel = new AuthorizeViewModel();
                await SetIdProviders(viewModel).ConfigureAwait(false);
                return View("Index", viewModel);
            }
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
            var uiLocales = DefaultLanguage;
            try
            {
                // 1. Decrypt the request
                var request = _dataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);

                // 2. Retrieve the default language
                uiLocales = string.IsNullOrWhiteSpace(request.UiLocales) ? DefaultLanguage : request.UiLocales;

                // 3. Check the state of the view model
                if (!ModelState.IsValid)
                {
                    await TranslateView(uiLocales).ConfigureAwait(false);
                    await SetIdProviders(viewModel).ConfigureAwait(false);
                    return View("OpenId", viewModel);
                }

                // 4. Local authentication
                var issuerName = Request.GetAbsoluteUriWithVirtualPath();
                var actionResult = await _authenticateActions.LocalOpenIdUserAuthentication(
                        new LocalAuthenticationParameter
                        {
                            UserName = viewModel.Login,
                            Password = viewModel.Password
                        },
                        request.ToParameter(),
                        viewModel.Code,
                        issuerName)
                    .ConfigureAwait(false);
                var subject = actionResult.Claims
                    .First(c => c.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject)
                    .Value;

                // 5. Two factor authentication.
                if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
                {
                    try
                    {
                        await SetTwoFactorCookie(actionResult.Claims).ConfigureAwait(false);
                        var code = await _authenticateActions.GenerateAndSendCode(subject).ConfigureAwait(false);
                        _simpleIdentityServerEventSource.GetConfirmationCode(code);
                        return RedirectToAction("SendCode", new {code = viewModel.Code});
                    }
                    catch (ClaimRequiredException)
                    {
                        return RedirectToAction("SendCode", new {code = viewModel.Code});
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("invalid_credentials",
                            "Two factor authenticator is not properly configured");
                    }
                }
                else
                {
                    // 6. Authenticate the user by adding a cookie
                    await SetLocalCookie(actionResult.Claims, request.SessionId).ConfigureAwait(false);
                    _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);

                    // 7. Redirect the user agent
                    var result = this.CreateRedirectionFromActionResult(actionResult.EndpointResult,
                        request);
                    if (result != null)
                    {
                        LogAuthenticateUser(actionResult.EndpointResult, request.ProcessId);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _simpleIdentityServerEventSource.Failure(ex.Message);
                ModelState.AddModelError("invalid_credentials", ex.Message);
            }

            await TranslateView(uiLocales).ConfigureAwait(false);
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return View("OpenId", viewModel);
        }
    }
}
