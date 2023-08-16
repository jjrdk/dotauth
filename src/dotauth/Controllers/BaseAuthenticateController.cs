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
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Exceptions;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Openid;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.ViewModels;
using DotAuth.WebSite.Authenticate;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the base authentication controller.
/// </summary>
/// <seealso cref="BaseController" />
public abstract class BaseAuthenticateController : BaseController
{
    private const string ExternalAuthenticateCookieName = "ExternalAuth-{0}";
    private readonly GenerateAndSendCodeAction _generateAndSendCode;
    private readonly ValidateConfirmationCodeAction _validateConfirmationCode;
    private readonly AuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenId;
    private readonly AuthenticateHelper _authenticateHelper;

    /// <summary>
    /// The data protector
    /// </summary>
    protected readonly IDataProtector DataProtector;

    private readonly IUrlHelper _urlHelper;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
    private readonly RuntimeSettings _runtimeSettings;
    private readonly IResourceOwnerStore _resourceOwnerRepository;
    private readonly IConfirmationCodeStore _confirmationCodeStore;
    private readonly ILogger _logger;
    private readonly AddUserOperation _addUser;
    private readonly GetUserOperation _getUserOperation;
    private readonly UpdateUserClaimsOperation _updateUserClaimsOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAuthenticateController"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="authenticationSchemeProvider">The authentication scheme provider.</param>
    /// <param name="twoFactorAuthenticationHandler">The two factor authentication handler.</param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="consentRepository">The consent repository.</param>
    /// <param name="scopeRepository">The scope repository.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="resourceOwnerRepository">The resource owner repository.</param>
    /// <param name="confirmationCodeStore">The confirmation code store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="jwksStore"></param>
    /// <param name="subjectBuilder"></param>
    /// <param name="accountFilters">The account filters.</param>
    /// <param name="logger">The controller logger.</param>
    /// <param name="runtimeSettings">The runtime settings.</param>
    protected BaseAuthenticateController(
        IDataProtectionProvider dataProtectionProvider,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IEventPublisher eventPublisher,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        IAuthorizationCodeStore authorizationCodeStore,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        ITokenStore tokenStore,
        IResourceOwnerRepository resourceOwnerRepository,
        IConfirmationCodeStore confirmationCodeStore,
        IClientStore clientStore,
        IJwksStore jwksStore,
        ISubjectBuilder subjectBuilder,
        IEnumerable<IAccountFilter> accountFilters,
        ILogger logger,
        RuntimeSettings runtimeSettings)
        : base(authenticationService)
    {
        _generateAndSendCode = new GenerateAndSendCodeAction(
            resourceOwnerRepository,
            confirmationCodeStore,
            twoFactorAuthenticationHandler,
            logger);
        _validateConfirmationCode = new ValidateConfirmationCodeAction(confirmationCodeStore);
        _authenticateHelper = new AuthenticateHelper(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
        _authenticateResourceOwnerOpenId = new AuthenticateResourceOwnerOpenIdAction(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
        DataProtector = dataProtectionProvider.CreateProtector("Request");
        _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);
        _eventPublisher = eventPublisher;
        _authenticationSchemeProvider = authenticationSchemeProvider;
        _addUser = new AddUserOperation(
            runtimeSettings,
            resourceOwnerRepository,
            accountFilters,
            subjectBuilder,
            eventPublisher);
        _getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        _updateUserClaimsOperation = new UpdateUserClaimsOperation(resourceOwnerRepository, logger);
        _runtimeSettings = runtimeSettings;
        _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
        _resourceOwnerRepository = resourceOwnerRepository;
        _confirmationCodeStore = confirmationCodeStore;
        _logger = logger;
    }

    /// <summary>
    /// Logs out this instance.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Response.Cookies.Delete(CoreConstants.SessionId);
        await _authenticationService.SignOutAsync(
                HttpContext,
                CookieNames.CookieName,
                new AuthenticationProperties())
            .ConfigureAwait(false);
        return Redirect("/");
    }

    /// <summary>
    /// Performs an external login.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">provider</exception>
    [HttpPost]
    public async Task ExternalLogin(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var redirectUrl = _urlHelper.Action("LoginCallback", "Authenticate", null, Request.Scheme);
        await _authenticationService.ChallengeAsync(
                HttpContext,
                provider,
                new AuthenticationProperties { RedirectUri = redirectUrl })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the login callback.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> LoginCallback(string error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogError("{error}", string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            return SetRedirection(string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error), Strings.InternalServerError, ErrorCodes.InternalError);
        }

        // 1. Get the authenticated user.
        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this)
            .ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return RedirectToAction("Index", "Authenticate");
        }

        var externalSubject = authenticatedUser.GetSubject()!;
        var resourceOwner = await _resourceOwnerRepository.Get(
                new ExternalAccountLink
                {
                    Issuer = authenticatedUser.Identity!.AuthenticationType!,
                    Subject = externalSubject
                },
                cancellationToken)
            .ConfigureAwait(false);
        // 2. Automatically create the resource owner.

        var claims = authenticatedUser.Claims.ToList();
        if (resourceOwner != null)
        {
            claims = resourceOwner.Claims.ToList();
        }
        else
        {
            var (subject, statusCode, s) =
                await AddExternalUser(authenticatedUser, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(subject))
            {
                return RedirectToAction("Index", "Error", new { code = statusCode!.Value, message = s });
            }

            var nameIdentifier = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                claims.Remove(nameIdentifier);
            }
            claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
            resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
        }

        await _authenticationService.SignOutAsync(
                HttpContext,
                null,
                new AuthenticationProperties())
            .ConfigureAwait(false);

        // 3. Two factor authentication.
        if (resourceOwner != null && !string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
        {
            await SetTwoFactorCookie(claims.ToArray()).ConfigureAwait(false);
            try
            {
                await _generateAndSendCode.Send(resourceOwner.Subject, cancellationToken).ConfigureAwait(false);
                return RedirectToAction("SendCode");
            }
            catch (ClaimRequiredException)
            {
                return RedirectToAction("SendCode");
            }
        }

        // 4. Set cookie
        await SetLocalCookie(claims.ToOpenidClaims(), Id.Create()).ConfigureAwait(false);
        await _authenticationService.SignOutAsync(
                HttpContext,
                null,
                new AuthenticationProperties())
            .ConfigureAwait(false);

        // 5. Redirect to the profile
        return Request.Query.TryGetValue("ReturnUrl", out var returnUrl)
            ? Redirect(returnUrl!)
            : RedirectToAction("Index", "User");
    }

    /// <summary>
    /// Sends the code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> SendCode(string code, CancellationToken cancellationToken)
    {
        // 1. Retrieve user
        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this, CookieNames.TwoFactorCookieName)
            .ConfigureAwait(false);
        if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
        {
            _logger.LogError("{error}", Strings.TwoFactorAuthenticationCannotBePerformed);
            return SetRedirection(
                Strings.TwoFactorAuthenticationCannotBePerformed,
                Strings.InternalServerError,
                ErrorCodes.UnhandledExceptionCode);
        }

        // 2. Return translated view.
        var resourceOwnerOption =
            await _getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
        if (resourceOwnerOption is Option<ResourceOwner>.Error)
        {
            return BadRequest();
        }

        var resourceOwner = ((Option<ResourceOwner>.Result)resourceOwnerOption).Item;

        var service = resourceOwner.TwoFactorAuthentication == null
            ? null
            : _twoFactorAuthenticationHandler.Get(resourceOwner.TwoFactorAuthentication);
        if (service == null)
        {
            return BadRequest();
        }

        var viewModel = new CodeViewModel { AuthRequestCode = code, ClaimName = service.RequiredClaim };
        var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == service.RequiredClaim);
        if (claim != null)
        {
            viewModel.ClaimValue = claim.Value;
        }

        ViewBag.IsAuthenticated = false;
        return Ok(viewModel);
    }

    /// <summary>
    /// Sends the code.
    /// </summary>
    /// <param name="codeViewModel">The code view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">codeViewModel</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCode(CodeViewModel codeViewModel, CancellationToken cancellationToken)
    {
        if (codeViewModel == null)
        {
            throw new ArgumentNullException(nameof(codeViewModel));
        }

        ViewBag.IsAuthenticated = false;
        codeViewModel.Validate(ModelState);
        if (!ModelState.IsValid)
        {
            return Ok(codeViewModel);
        }

        // 1. Check user is authenticated
        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this, CookieNames.TwoFactorCookieName)
            .ConfigureAwait(false);
        if (authenticatedUser?.Identity?.IsAuthenticated != true)
        {
            _logger.LogError("{error}", Strings.TwoFactorAuthenticationCannotBePerformed);
            return BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = Strings.TwoFactorAuthenticationCannotBePerformed,
                    Status = HttpStatusCode.BadRequest
                });
        }

        // 2. Resend the confirmation code.
        var subject = authenticatedUser.GetSubject()!;
        if (codeViewModel.Action == CodeViewModel.ResendAction)
        {
            var option = await _getUserOperation.Execute(authenticatedUser, cancellationToken)
                .ConfigureAwait(false);
            if (option is not Option<ResourceOwner>.Result ro)
            {
                return BadRequest();
            }

            var resourceOwner = ro.Item;
            var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == codeViewModel.ClaimName);
            if (claim != null)
            {
                resourceOwner.Claims.Remove(claim);
            }

            resourceOwner.Claims =
                resourceOwner.Claims.Add(new Claim(codeViewModel.ClaimName!, codeViewModel.ClaimValue!));
            var claimsLst = resourceOwner.Claims.Select(c => new Claim(c.Type, c.Value));
            await _updateUserClaimsOperation.Execute(subject, claimsLst, cancellationToken).ConfigureAwait(false);
            await _generateAndSendCode.Send(subject, cancellationToken).ConfigureAwait(false);
            return Ok(codeViewModel);
        }

        // 3. Validate the confirmation code
        if (!await _validateConfirmationCode.Execute(codeViewModel.Code!, subject, cancellationToken)
                .ConfigureAwait(false))
        {
            _logger.LogError(
                "Two factor authentication failed for subject: {subject}, auth request: {authRequestCode}, code: {code}",
                subject,
                codeViewModel.AuthRequestCode,
                codeViewModel.Code);
            await _eventPublisher.Publish(
                new TwoFactorAuthenticationFailed(
                    Id.Create(),
                    subject,
                    codeViewModel.AuthRequestCode,
                    codeViewModel.Code,
                    DateTimeOffset.UtcNow)).ConfigureAwait(false);
            ModelState.AddModelError("Code", "confirmation code is not valid");
            return Ok(codeViewModel);
        }

        // 4. Remove the code
        if (string.IsNullOrWhiteSpace(codeViewModel.Code)
            || !await _confirmationCodeStore.Remove(codeViewModel.Code, subject, cancellationToken)
                .ConfigureAwait(false))
        {
            ModelState.AddModelError("Code", "an error occurred while trying to remove the code");
            return Ok(codeViewModel);
        }

        // 5. Authenticate the resource owner
        await _authenticationService.SignOutAsync(
                HttpContext,
                CookieNames.TwoFactorCookieName,
                new AuthenticationProperties())
            .ConfigureAwait(false);

        // 6. Redirect the user agent
        var authenticatedUserClaims = authenticatedUser.Claims.ToArray();
        if (!string.IsNullOrWhiteSpace(codeViewModel.AuthRequestCode))
        {
            var request = DataProtector.Unprotect<AuthorizationRequest>(codeViewModel.AuthRequestCode);
            if (request.session_id == null)
            {
                return BadRequest();
            }

            await SetLocalCookie(authenticatedUserClaims, request.session_id).ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _authenticateHelper.ProcessRedirection(
                    request.ToParameter(),
                    codeViewModel.AuthRequestCode,
                    subject,
                    authenticatedUserClaims,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            await LogAuthenticateUser(subject, actionResult.Amr).ConfigureAwait(false);
            var result = actionResult.CreateRedirectionFromActionResult(request, _logger)!;
            return result;
        }

        await SetLocalCookie(authenticatedUserClaims, Id.Create()).ConfigureAwait(false);

        // 7. Redirect the user agent to the User view.
        return RedirectToAction("Index", "User");
    }

    /// <summary>
    /// Logs in uses OpenID.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">code</exception>
    [HttpGet]
    public async Task<IActionResult> OpenId(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            var vm = new AuthorizeOpenIdViewModel { Code = code };

            await SetIdProviders(vm).ConfigureAwait(false);
            return Ok(vm);
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        var dataString = Uri.UnescapeDataString(code);
        var request = DataProtector.Unprotect<AuthorizationRequest>(dataString);
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await _authenticateResourceOwnerOpenId.Execute(
                request.ToParameter(),
                authenticatedUser!,
                code,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        var result = actionResult.CreateRedirectionFromActionResult(request, _logger);
        if (result != null)
        {
            await LogAuthenticateUser(authenticatedUser.GetSubject()!, actionResult.Amr).ConfigureAwait(false);
            return result;
        }

        var viewModel = new AuthorizeOpenIdViewModel { Code = code };

        await SetIdProviders(viewModel).ConfigureAwait(false);
        return Ok(viewModel);
    }

    /// <summary>
    /// Logs in using external OpenID.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="code">The code.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">code</exception>
    [HttpPost]
    public async Task ExternalLoginOpenId(string provider, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentNullException(nameof(code));
        }

        // 1. Persist the request code into a cookie & fix the space problems
        var cookieValue = Id.Create();
        var cookieName = string.Format(ExternalAuthenticateCookieName, cookieValue);
        Response.Cookies.Append(
            cookieName,
            code,
            new CookieOptions
            {
                //SameSite = SameSiteMode.Unspecified,
                Secure = !_runtimeSettings.AllowHttp,
                HttpOnly = _runtimeSettings.AllowHttp,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            });

        // 2. Redirect the User agent
        var redirectUrl = _urlHelper.Action(
            "LoginCallbackOpenId",
            "Authenticate",
            new { code = cookieValue },
            Request.Scheme);
        await _authenticationService.ChallengeAsync(
                HttpContext,
                provider,
                new AuthenticationProperties { RedirectUri = redirectUrl })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the local OpenID callback.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="error">The error.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">code</exception>
    [HttpGet]
    public async Task<IActionResult> LoginCallbackOpenId(
        string code,
        string error,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentNullException(nameof(code));
        }

        // 1 : retrieve the request from the cookie
        var cookieName = string.Format(ExternalAuthenticateCookieName, code);
        var request = Request.Cookies[cookieName];
        if (request == null)
        {
            _logger.LogError("{error}", Strings.TheRequestCannotBeExtractedFromTheCookie);
            return SetRedirection(
                Strings.TheRequestCannotBeExtractedFromTheCookie,
                Strings.InternalServerError,
                ErrorCodes.UnhandledExceptionCode);
        }

        // 2 : remove the cookie
        Response.Cookies.Append(
            cookieName,
            string.Empty,
            new CookieOptions
            {
                HttpOnly = _runtimeSettings.AllowHttp,
                Secure = !_runtimeSettings.AllowHttp,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

        // 3 : Raise an exception is there's an authentication error
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogError("{error}", string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            return SetRedirection(
                string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error),
                Strings.InternalServerError,
                ErrorCodes.UnhandledExceptionCode);
        }

        // 4. Check if the user is authenticated
        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this)
            .ConfigureAwait(false);
        if (authenticatedUser?.Identity?.IsAuthenticated != true || authenticatedUser.Identity is not ClaimsIdentity)
        {
            _logger.LogError("{msg}", Strings.TheUserNeedsToBeAuthenticated);
            return SetRedirection(Strings.TheUserNeedsToBeAuthenticated, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        // 5. Retrieve the claims & insert the resource owner if needed.
        //var claimsIdentity = authenticatedUser.Identity as ClaimsIdentity;
        var claims = authenticatedUser.Claims.ToArray();
        var externalSubject = authenticatedUser.GetSubject();
        var resourceOwner = await _resourceOwnerRepository.Get(
                new ExternalAccountLink
                {
                    Issuer = authenticatedUser.Identity!.AuthenticationType!,
                    Subject = externalSubject!
                },
                cancellationToken)
            .ConfigureAwait(false);
        var sub = string.Empty;
        if (resourceOwner == null)
        {
            var (s, statusCode, error1) =
                await AddExternalUser(authenticatedUser, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(s))
            {
                return RedirectToAction("Index", "Error", new { code = statusCode!.Value, message = error1 });
            }

            sub = s;
            resourceOwner = await _resourceOwnerRepository.Get(s, cancellationToken).ConfigureAwait(false);
        }

        if (resourceOwner != null)
        {
            claims = resourceOwner.Claims;
        }
        else
        {
            var nameIdentifier = claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            claims = claims.Remove(nameIdentifier);
            claims = claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));
        }

        if (resourceOwner != null && !string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
        {
            await SetTwoFactorCookie(claims).ConfigureAwait(false);
            await _generateAndSendCode.Send(resourceOwner.Subject, cancellationToken).ConfigureAwait(false);
            return RedirectToAction("SendCode", new { code = request });
        }

        var subject = resourceOwner!.Subject;
        // 6. Try to authenticate the resource owner & returns the claims.
        var authorizationRequest = DataProtector.Unprotect<AuthorizationRequest>(request);
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationRequest.ToParameter(),
                request,
                subject,
                claims,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);

        // 7. Store claims into new cookie
        await SetLocalCookie(claims.ToOpenidClaims(), authorizationRequest.session_id!).ConfigureAwait(false);
        await _authenticationService.SignOutAsync(
                HttpContext,
                null,
                new AuthenticationProperties())
            .ConfigureAwait(false);
        await LogAuthenticateUser(subject, actionResult.Amr!).ConfigureAwait(false);
        return actionResult.CreateRedirectionFromActionResult(authorizationRequest, _logger)!;
    }

    /// <summary>
    /// Sets the identifier providers.
    /// </summary>
    /// <param name="authorizeViewModel">The authorize view model.</param>
    /// <returns></returns>
    protected async Task SetIdProviders(IdProviderAuthorizeViewModel authorizeViewModel)
    {
        var schemes = (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(
            p => !string.IsNullOrWhiteSpace(p.DisplayName) && !p.DisplayName.StartsWith('_'));
        var idProviders = schemes.Select(
                scheme => new IdProviderViewModel
                {
                    AuthenticationScheme = scheme.Name,
                    DisplayName = scheme.DisplayName
                })
            .ToArray();

        authorizeViewModel.IdProviders = idProviders;
    }

    /// <summary>
    /// Logs the authenticate user.
    /// </summary>
    /// <param name="resourceOwner">The resource owner.</param>
    /// <param name="amr">The amr.</param>
    /// <returns></returns>
    internal async Task LogAuthenticateUser(string resourceOwner, string? amr)
    {
        await _eventPublisher
            .Publish(new ResourceOwnerAuthenticated(Id.Create(), resourceOwner, amr, DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the local cookie.
    /// </summary>
    /// <param name="claims">The claims.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns></returns>
    protected async Task SetLocalCookie(Claim[] claims, string sessionId)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(_runtimeSettings.RptLifeTime);
        Response.Cookies.Append(
            CoreConstants.SessionId,
            sessionId,
            new CookieOptions
            {
                HttpOnly = _runtimeSettings.AllowHttp,
                Secure = !_runtimeSettings.AllowHttp,
                Expires = expires,
                SameSite = SameSiteMode.Strict
            });
        var identity = new ClaimsIdentity(claims, CookieNames.CookieName);
        var principal = new ClaimsPrincipal(identity);
        await _authenticationService.SignInAsync(
                HttpContext,
                CookieNames.CookieName,
                principal,
                new AuthenticationProperties
                {
                    IssuedUtc = now,
                    ExpiresUtc = expires,
                    AllowRefresh = false,
                    IsPersistent = false
                })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the two factor cookie.
    /// </summary>
    /// <param name="claims">The claims.</param>
    /// <returns></returns>
    protected async Task SetTwoFactorCookie(Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, CookieNames.TwoFactorCookieName);
        var principal = new ClaimsPrincipal(identity);
        await _authenticationService.SignInAsync(
                HttpContext,
                CookieNames.TwoFactorCookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                    IsPersistent = false
                })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Add an external account.
    /// </summary>
    /// <param name="authenticatedUser"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    private async Task<(string? subject, int? statusCode, string? error)> AddExternalUser(
        ClaimsPrincipal authenticatedUser,
        CancellationToken cancellationToken)
    {
        var externalClaims = authenticatedUser.Claims.Where(c => !string.IsNullOrWhiteSpace(c.Value)).ToArray();
        var userClaims = _runtimeSettings.ClaimsIncludedInUserCreation
            .Except(externalClaims.Select(x => x.Type).ToOpenIdClaimType())
            .Select(x => new Claim(x, string.Empty))
            .Concat(externalClaims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)))
            .Concat(
                externalClaims.Any(x => x.Type == OpenIdClaimTypes.Email)
                    ? new[]
                    {
                        new Claim(
                            "domain",
                            externalClaims.First(x => x.Type == OpenIdClaimTypes.Email)
                                .Value[externalClaims.First(x => x.Type == OpenIdClaimTypes.Email)
                                    .Value.LastIndexOf('@')..])
                    }
                    : Array.Empty<Claim>())
            .ToOpenidClaims()
            .OrderBy(x => x.Type)
            .ToArray();

        var now = DateTimeOffset.UtcNow;
        var record = new ResourceOwner
        {
            Subject = Id.Create(),
            ExternalLogins =
                new[]
                {
                    new ExternalAccountLink
                    {
                        Subject = authenticatedUser.GetSubject()!,
                        Issuer = authenticatedUser.Identity!.AuthenticationType!,
                        ExternalClaims = authenticatedUser.Claims
                            .Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer))
                            .ToArray()
                    }
                },
            Password = Id.Create().ToSha256Hash(string.Empty),
            IsLocalAccount = false,
            Claims = userClaims,
            CreateDateTime = now,
            UpdateDateTime = now,
            TwoFactorAuthentication = null
        };

        var (success, subject) = await _addUser.Execute(record, cancellationToken).ConfigureAwait(false);
        if (success)
        {
            record.Password = string.Empty;
            await _eventPublisher.Publish(new ExternalUserCreated(Id.Create(), record, now))
                .ConfigureAwait(false);
        }

        return (subject, null, null);
    }
}
