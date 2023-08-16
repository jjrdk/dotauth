﻿namespace DotAuth.Sms.Controllers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Controllers;
using DotAuth.Events;
using DotAuth.Exceptions;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Logging;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Sms.Actions;
using DotAuth.Sms.Properties;
using DotAuth.Sms.ViewModels;
using DotAuth.ViewModels;
using DotAuth.WebSite.Authenticate;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the authenticate controller.
/// </summary>
/// <seealso cref="BaseAuthenticateController" />
[Area(SmsConstants.Amr)]
[ThrottleFilter]
public sealed class AuthenticateController : BaseAuthenticateController
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfirmationCodeStore _confirmationCodeStore;
    private readonly ILogger<AuthenticateController> _logger;
    private readonly GetUserOperation _getUserOperation;
    private readonly SmsAuthenticationOperation _smsAuthenticationOperation;
    private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
    private readonly ValidateConfirmationCodeAction _validateConfirmationCode;
    private readonly AuthenticateHelper _authenticateHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateController"/> class.
    /// </summary>
    /// <param name="smsClient">The SMS client.</param>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="authenticationSchemeProvider">The authentication scheme provider.</param>
    /// <param name="twoFactorAuthenticationHandler">The two factor authentication handler.</param>
    /// <param name="subjectBuilder">The subject builder.</param>
    /// <param name="consentRepository">The consent repository.</param>
    /// <param name="scopeRepository">The scope repository.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="resourceOwnerRepository">The resource owner repository.</param>
    /// <param name="confirmationCodeStore">The confirmation code store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="jwksStore">The JWKS store.</param>
    /// <param name="accountFilters">The account filters.</param>
    /// <param name="logger">The controller logger.</param>
    /// <param name="runtimeSettings">The runtime settings.</param>
    public AuthenticateController(
        ISmsClient smsClient,
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
        IJwksStore jwksStore,
        IEnumerable<IAccountFilter> accountFilters,
        ILogger<AuthenticateController> logger,
        RuntimeSettings runtimeSettings)
        : base(
            dataProtectionProvider,
            urlHelperFactory,
            actionContextAccessor,
            eventPublisher,
            authenticationService,
            authenticationSchemeProvider,
            twoFactorAuthenticationHandler,
            authorizationCodeStore,
            consentRepository,
            scopeRepository,
            tokenStore,
            resourceOwnerRepository,
            confirmationCodeStore,
            clientStore,
            jwksStore,
            subjectBuilder,
            accountFilters,
            logger,
            runtimeSettings)
    {
        _eventPublisher = eventPublisher;
        _confirmationCodeStore = confirmationCodeStore;
        _logger = logger;
        _getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        var generateSms = new GenerateAndSendSmsCodeOperation(smsClient, confirmationCodeStore, logger);
        _smsAuthenticationOperation = new SmsAuthenticationOperation(
            runtimeSettings,
            smsClient,
            confirmationCodeStore,
            resourceOwnerRepository,
            subjectBuilder,
            accountFilters.ToArray(),
            eventPublisher,
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
        _generateAndSendSmsCodeOperation = generateSms;
    }

    /// <summary>
    /// Get the default page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return Ok(viewModel);
        }

        return RedirectToAction("Index", "User", new { area = "pwd" });
    }

    /// <summary>
    /// Does a local login.
    /// </summary>
    /// <param name="localAuthenticationViewModel">The local authentication view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">localAuthenticationViewModel</exception>
    [HttpPost]
    public async Task<IActionResult> LocalLogin(
        SmsAuthenticationViewModel localAuthenticationViewModel,
        CancellationToken cancellationToken)
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser?.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "User", new { Area = "pwd" });
        }

        if (localAuthenticationViewModel == null)
        {
            throw new ArgumentNullException(nameof(localAuthenticationViewModel));
        }

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(localAuthenticationViewModel.PhoneNumber))
        {
            ResourceOwner? resourceOwner = null;
            var option = string.IsNullOrWhiteSpace(localAuthenticationViewModel.PhoneNumber)
                ? new Option<ResourceOwner>.Error(new ErrorDetails
                {
                    Title = "Invalid phone number", Detail = "Phone number is required",
                    Status = HttpStatusCode.BadRequest
                })
                : await _smsAuthenticationOperation
                    .Execute(localAuthenticationViewModel.PhoneNumber, cancellationToken)
                    .ConfigureAwait(false);
            if (option is Option<ResourceOwner>.Error e)
            {
                await _eventPublisher.Publish(
                        new DotAuthError(
                            DotAuth.Id.Create(),
                            e.Details.Title,
                            e.Details.Detail,
                            string.Empty,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
                ModelState.AddModelError("message_error", e.Details.Detail);
            }
            else
            {
                resourceOwner = ((Option<ResourceOwner>.Result)option).Item;
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
                    await _eventPublisher.Publish(
                            new DotAuthError(
                                DotAuth.Id.Create(),
                                ex.Message,
                                ex.Message,
                                string.Empty,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);
                    ModelState.AddModelError("message_error", "SMS account is not valid");
                }
            }
        }

        var viewModel = new AuthorizeViewModel();
        await SetIdProviders(viewModel).ConfigureAwait(false);
        RouteData.Values["view"] = "Index";
        return Ok(viewModel);
    }

    /// <summary>
    /// Confirms the code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ConfirmCode(string code)
    {
        var user = await SetUser().ConfigureAwait(false);
        if (user?.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "User", new { Area = "pwd" });
        }

        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this, CookieNames.PasswordLessCookieName)
            .ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            var message = "SMS authentication cannot be performed";
            _logger.LogError(message);
            return SetRedirection(message, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        return Ok(new ConfirmCodeViewModel { Code = code });
    }

    /// <summary>
    /// Confirms the code.
    /// </summary>
    /// <param name="confirmCodeViewModel">The confirm code view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">confirmCodeViewModel</exception>
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
        if (user?.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "User", new { Area = "pwd" });
        }

        var authenticatedUser = await _authenticationService
            .GetAuthenticatedUser(this, CookieNames.PasswordLessCookieName)
            .ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            var message = "SMS authentication cannot be performed";
            _logger.LogError(message);
            return SetRedirection(message, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        var authenticatedUserClaims = authenticatedUser.Claims.ToArray();
        var subject = authenticatedUserClaims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;
        var phoneNumber = authenticatedUserClaims.First(c => c.Type == OpenIdClaimTypes.PhoneNumber);
        if (confirmCodeViewModel.Action == "resend") // Resend the confirmation code.
        {
            _ = await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, cancellationToken)
                .ConfigureAwait(false);
            return Ok(confirmCodeViewModel);
        }

        // Check the confirmation code.
        if (confirmCodeViewModel.ConfirmationCode == null
         || !await _validateConfirmationCode
                .Execute(confirmCodeViewModel.ConfirmationCode, subject, cancellationToken)
                .ConfigureAwait(false))
        {
            ModelState.AddModelError("message_error", "Confirmation code is not valid");
            return Ok(confirmCodeViewModel);
        }

        await _authenticationService.SignOutAsync(
                HttpContext,
                CookieNames.PasswordLessCookieName,
                new AuthenticationProperties())
            .ConfigureAwait(false);
        var resourceOwnerOption =
            await _getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
        if (resourceOwnerOption is Option<ResourceOwner>.Error e)
        {
            return SetRedirection(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
        }

        var resourceOwner = (resourceOwnerOption as Option<ResourceOwner>.Result)!.Item;
        if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication)) // Execute TWO Factor authentication
        {
            try
            {
                await SetTwoFactorCookie(authenticatedUserClaims).ConfigureAwait(false);
                await _generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, cancellationToken)
                    .ConfigureAwait(false);
                return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
            }
            catch (ClaimRequiredException)
            {
                return RedirectToAction("SendCode", new { code = confirmCodeViewModel.Code });
            }
            catch (Exception)
            {
                ModelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                return Ok(confirmCodeViewModel);
            }
        }

        if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)) // Execute OPENID workflow
        {
            var request = DataProtector.Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
            await SetLocalCookie(authenticatedUserClaims, request.session_id!).ConfigureAwait(false);
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _authenticateHelper.ProcessRedirection(
                    request.ToParameter(),
                    confirmCodeViewModel.Code,
                    subject,
                    authenticatedUserClaims,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            var result = actionResult.CreateRedirectionFromActionResult(request, _logger);
            if (result != null)
            {
                await LogAuthenticateUser(resourceOwner.Subject, actionResult.Amr!).ConfigureAwait(false);
                return result;
            }
        }

        await SetLocalCookie(authenticatedUserClaims, DotAuth.Id.Create())
            .ConfigureAwait(false); // Authenticate the resource owner
        var modelCode = string.IsNullOrWhiteSpace(confirmCodeViewModel.Code)
            ? confirmCodeViewModel.ConfirmationCode
            : confirmCodeViewModel.Code;

        if (!string.IsNullOrWhiteSpace(modelCode))
        {
            await _confirmationCodeStore.Remove(modelCode, subject, cancellationToken).ConfigureAwait(false);
        }

        return RedirectToAction("Index", "User", new { Area = "pwd" });
    }

    /// <summary>
    /// Does the login with OpenID.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// viewModel
    /// or
    /// Code
    /// </exception>
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
            throw new ArgumentException(ErrorMessages.InvalidCode, nameof(viewModel));
        }

        await SetUser().ConfigureAwait(false);
        // 1. Decrypt the request
        // 2. Retrieve the default language
        if (ModelState.IsValid)
        {
            ResourceOwner? resourceOwner = null;

            if (string.IsNullOrWhiteSpace(viewModel.PhoneNumber))
            {
                ModelState.AddModelError("message_error", SmsStrings.MissingPhoneNumber);
            }
            else
            {
                var option = string.IsNullOrWhiteSpace(viewModel.PhoneNumber)
                    ? new Option<ResourceOwner>.Error(new ErrorDetails
                    {
                        Title = "Invalid phone number", Detail = "Phone number is required",
                        Status = HttpStatusCode.BadRequest
                    })
                    : await _smsAuthenticationOperation.Execute(viewModel.PhoneNumber, cancellationToken)
                        .ConfigureAwait(false);
                if (option is Option<ResourceOwner>.Error ex)
                {
                    await _eventPublisher.Publish(
                            new DotAuthError(
                                DotAuth.Id.Create(),
                                ex.Details.Title,
                                ex.Details.Detail,
                                string.Empty,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);
                    ModelState.AddModelError("message_error", ex.Details.Detail);
                }
                else
                {
                    resourceOwner = ((Option<ResourceOwner>.Result)option).Item;
                }
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
                    await _eventPublisher.Publish(
                            new DotAuthError(
                                DotAuth.Id.Create(),
                                ex.Message,
                                ex.Message,
                                string.Empty,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);
                    ModelState.AddModelError("message_error", "SMS account is not valid");
                }
            }
        }

        await SetIdProviders(viewModel).ConfigureAwait(false);
        RouteData.Values["view"] = "OpenId";
        return Ok(viewModel);
    }

    private async Task SetPasswordLessCookie(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, CookieNames.PasswordLessCookieName);
        var principal = new ClaimsPrincipal(identity);
        await _authenticationService.SignInAsync(
                HttpContext,
                CookieNames.PasswordLessCookieName,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
                    IsPersistent = false
                })
            .ConfigureAwait(false);
    }
}
