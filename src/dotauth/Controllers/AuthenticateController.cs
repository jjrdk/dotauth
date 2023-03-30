namespace DotAuth.Controllers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Exceptions;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Logging;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.ViewModels;
using DotAuth.WebSite.Authenticate;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the authentication controller.
/// </summary>
/// <seealso cref="BaseAuthenticateController" />
[ThrottleFilter]
public sealed class AuthenticateController : BaseAuthenticateController
{
    private const string InvalidCredentials = "invalid_credentials";
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuthenticateController> _logger;
    private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;
    private readonly LocalOpenIdUserAuthenticationAction _localOpenIdAuthentication;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateController"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="authenticationSchemeProvider">The authentication scheme provider.</param>
    /// <param name="resourceOwnerServices">The resource owner services.</param>
    /// <param name="twoFactorAuthenticationHandler">The two factor authentication handler.</param>
    /// <param name="subjectBuilder">The subject builder.</param>
    /// <param name="authorizationCodeStore">The authorization code store.</param>
    /// <param name="scopeRepository">The scope repository.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="consentRepository">The consent repository.</param>
    /// <param name="confirmationCodeStore">The confirmation code store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="resourceOwnerRepository">The resource owner repository.</param>
    /// <param name="jwksStore"></param>
    /// <param name="accountFilters">The account filters.</param>
    /// <param name="logger">The controller logger.</param>
    /// <param name="runtimeSettings">The runtime settings.</param>
    public AuthenticateController(
        IDataProtectionProvider dataProtectionProvider,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IEventPublisher eventPublisher,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ISubjectBuilder subjectBuilder,
        IAuthorizationCodeStore authorizationCodeStore,
        IScopeRepository scopeRepository,
        ITokenStore tokenStore,
        IConsentRepository consentRepository,
        IConfirmationCodeStore confirmationCodeStore,
        IClientStore clientStore,
        IResourceOwnerRepository resourceOwnerRepository,
        IJwksStore jwksStore,
        IEnumerable<AccountFilter> accountFilters,
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
        _logger = logger;
        var services = resourceOwnerServices.ToArray();
        _resourceOwnerServices = services;
        _localOpenIdAuthentication = new LocalOpenIdUserAuthenticationAction(
            authorizationCodeStore,
            services,
            consentRepository,
            tokenStore,
            scopeRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Indexes the specified cancellation token.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        var hasReturnUrl = Request.Query.TryGetValue("ReturnUrl", out var returnUrl);
        if (authenticatedUser?.Identity == null || !authenticatedUser.Identity.IsAuthenticated)
        {
            var viewModel = new AuthorizeViewModel { ReturnUrl = returnUrl };
            await SetIdProviders(viewModel).ConfigureAwait(false);
            return Ok(viewModel);
        }

        return hasReturnUrl ? Redirect(returnUrl!) : RedirectToAction("Index", "User");
    }

    /// <summary>
    /// Handles the local login request.
    /// </summary>
    /// <param name="authorizeViewModel">The authorize view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">authorizeViewModel</exception>
    /// <exception cref="Exception">Two factor authenticator is not properly configured</exception>
    [HttpPost]
    public async Task<IActionResult> LocalLogin(
        [FromForm] LocalAuthenticationViewModel authorizeViewModel,
        CancellationToken cancellationToken)
    {
        if (authorizeViewModel.Login == null || authorizeViewModel.Password == null)
        {
            BadRequest();
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser?.Identity != null && authenticatedUser.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "User");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            RouteData.Values["view"] = "Index";
            return Ok(viewModel);
        }

        try
        {
            var resourceOwner = await _resourceOwnerServices.Authenticate(
                    authorizeViewModel.Login!,
                    authorizeViewModel.Password!,
                    cancellationToken)
                .ConfigureAwait(false);
            if (resourceOwner == null)
            {
                _logger.LogError(Strings.TheResourceOwnerCredentialsAreNotCorrect);
                var viewModel = new AuthorizeViewModel
                {
                    Password = authorizeViewModel.Password,
                    UserName = authorizeViewModel.Login
                };
                await SetIdProviders(viewModel).ConfigureAwait(false);
                RouteData.Values["view"] = "Index";
                return Ok(viewModel);
            }

            resourceOwner.Claims = resourceOwner.Claims.Add(
                new Claim(
                    ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
            var subject = resourceOwner.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;
            if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                await SetLocalCookie(resourceOwner.Claims, Id.Create()).ConfigureAwait(false);
                return !string.IsNullOrWhiteSpace(authorizeViewModel.ReturnUrl)
                    ? Redirect(authorizeViewModel.ReturnUrl)
                    : RedirectToAction("Index", "User");
            }

            // 2.1 Store temporary information in cookie
            await SetTwoFactorCookie(resourceOwner.Claims).ConfigureAwait(false);
            // 2.2. Send confirmation code
            try
            {
                await SendCode(subject, cancellationToken).ConfigureAwait(false);
                return RedirectToAction("SendCode");
            }
            catch (ClaimRequiredException cre)
            {
                await _eventPublisher.Publish(
                        new DotAuthError(
                            Id.Create(),
                            cre.Code,
                            cre.Message,
                            string.Empty,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
                return RedirectToAction("SendCode");
            }
            catch (Exception ex)
            {
                await _eventPublisher.Publish(
                        new DotAuthError(
                            Id.Create(),
                            "misconfigured_2fa",
                            ex.Message,
                            string.Empty,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
                throw new Exception(Strings.TwoFactorNotProperlyConfigured, ex);
            }
        }
        catch (Exception exception)
        {
            await _eventPublisher.Publish(
                    new DotAuthError(
                        Id.Create(),
                        InvalidCredentials,
                        exception.Message,
                        string.Empty,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            ModelState.AddModelError(InvalidCredentials, exception.Message);
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(viewModel).ConfigureAwait(false);
            RouteData.Values["view"] = "Index";
            return Ok(viewModel);
        }
    }

    /// <summary>
    /// Handles the local open id login.
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
        OpenidLocalAuthenticationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (viewModel == null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        if (string.IsNullOrWhiteSpace(viewModel.Code))
        {
            throw new ArgumentException(Strings.MissingValues, nameof(viewModel));
        }

        await SetUser().ConfigureAwait(false);

        // 1. Decrypt the request
        var request = DataProtector.Unprotect<AuthorizationRequest>(viewModel.Code);

        // 3. Check the state of the view model
        if (!ModelState.IsValid)
        {
            await SetIdProviders(viewModel).ConfigureAwait(false);
            RouteData.Values["view"] = "OpenId";
            return Ok(viewModel);
        }

        // 4. Local authentication
        var issuerName = Request.GetAbsoluteUriWithVirtualPath();

        var actionResult = await _localOpenIdAuthentication.Execute(
                new LocalAuthenticationParameter { UserName = viewModel.Login, Password = viewModel.Password },
                request.ToParameter(),
                viewModel.Code,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (actionResult.ErrorMessage == null)
        {
            var subject = actionResult.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;

            // 5. Two factor authentication.
            if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
            {
                try
                {
                    await SetTwoFactorCookie(actionResult.Claims).ConfigureAwait(false);
                    await SendCode(subject, cancellationToken).ConfigureAwait(false);
                    return RedirectToAction("SendCode", new { code = viewModel.Code });
                }
                catch (ClaimRequiredException cre)
                {
                    await _eventPublisher.Publish(
                            new DotAuthError(
                                Id.Create(),
                                cre.Code,
                                cre.Message,
                                string.Empty,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);
                    return RedirectToAction("SendCode", new { code = viewModel.Code });
                }
                catch (Exception ex)
                {
                    await _eventPublisher.Publish(
                            new DotAuthError(
                                Id.Create(),
                                ex.Message,
                                ex.Message,
                                string.Empty,
                                DateTimeOffset.UtcNow))
                        .ConfigureAwait(false);
                    ModelState.AddModelError(
                        InvalidCredentials,
                        "Two factor authenticator is not properly configured");
                }
            }
            else
            {
                // 6. Authenticate the user by adding a cookie
                await SetLocalCookie(actionResult.Claims, request.session_id!).ConfigureAwait(false);

                // 7. Redirect the user agent
                var endpointResult = actionResult.EndpointResult!;
                var result = endpointResult.CreateRedirectionFromActionResult(request, _logger);
                if (result != null)
                {
                    await LogAuthenticateUser(subject, endpointResult.Amr!).ConfigureAwait(false);
                    return result;
                }
            }
        }
        else
        {
            await _eventPublisher.Publish(
                    new DotAuthError(
                        Id.Create(),
                        ErrorCodes.InvalidRequest,
                        actionResult.ErrorMessage,
                        string.Empty,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            ModelState.AddModelError(InvalidCredentials, actionResult.ErrorMessage);
        }

        await SetIdProviders(viewModel).ConfigureAwait(false);
        RouteData.Values["view"] = "OpenId";
        return Ok(viewModel);
    }
}