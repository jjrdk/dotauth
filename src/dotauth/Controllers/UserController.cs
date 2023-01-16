namespace DotAuth.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.ViewModels;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles user related requests.
/// </summary>
/// <seealso cref="BaseController" />
[Authorize("authenticated")]
[ThrottleFilter]
public sealed class UserController : BaseController
{
    private readonly IResourceOwnerRepository _resourceOwnerRepository;
    private readonly GetUserOperation _getUserOperation;
    private readonly UpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
    private readonly IConsentRepository _consentRepository;
    private readonly IScopeRepository _scopeRepository;
    private readonly IUrlHelper _urlHelper;
    private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="resourceOwnerRepository">The resource owner repository.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="authenticationSchemeProvider">The authentication scheme provider.</param>
    /// <param name="urlHelperFactory">The URL helper factory.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    /// <param name="consentRepository">The consent repository.</param>
    /// <param name="scopeRepository"></param>
    /// <param name="twoFactorAuthenticationHandler">The two factor authentication handler.</param>
    /// <param name="logger">The logger</param>
    public UserController(
        IResourceOwnerRepository resourceOwnerRepository,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILogger<UserController> logger)
        : base(authenticationService)
    {
        _resourceOwnerRepository = resourceOwnerRepository;
        _getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        _updateUserTwoFactorAuthenticatorOperation =
            new UpdateUserTwoFactorAuthenticatorOperation(resourceOwnerRepository, logger);
        _authenticationSchemeProvider = authenticationSchemeProvider;
        _consentRepository = consentRepository;
        _scopeRepository = scopeRepository;
        _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);
        _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
        _logger = logger;
    }

    /// <summary>
    /// Displays the default consent view.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var subject = authenticatedUser.GetSubject();
        var ro = subject == null ? null : await GetUserProfile(subject, cancellationToken).ConfigureAwait(false);
        if (ro == null)
        {
            return BadRequest(
                new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InternalError,
                    Detail = Strings.TheRoDoesntExist
                });
        }

        var authenticationSchemes =
            (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(
                a => !string.IsNullOrWhiteSpace(a.DisplayName));
        var viewModel = new ProfileViewModel(ro.Claims);

        foreach (var profile in ro.ExternalLogins)
        {
            var record = new IdentityProviderViewModel(profile.Issuer, profile.Subject);
            viewModel.LinkedIdentityProviders.Add(record);
        }

        var actualScheme = authenticatedUser?.Identity?.AuthenticationType;

        viewModel.UnlinkedIdentityProviders = authenticationSchemes
            .Where(
                a => a.DisplayName != null
                     && !a.DisplayName.StartsWith('_')
                     && !ro.ExternalLogins.Any(p => p.Issuer == a.Name && a.Name != actualScheme))
            .Select(p => new IdentityProviderViewModel(p.Name))
            .ToList();
        return Ok(viewModel);
    }

    /// <summary>
    /// Consents the specified cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Consent(CancellationToken cancellationToken)
    {
        await SetUser().ConfigureAwait(false);
        return await GetConsents(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Consents the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Consent(string id, CancellationToken cancellationToken)
    {
        var removed = await _consentRepository.Delete(new Consent { Id = id }, cancellationToken)
            .ConfigureAwait(false);
        if (!removed)
        {
            ViewBag.ErrorMessage = Strings.TheConsentCannotBeDeleted;
            return await GetConsents(cancellationToken).ConfigureAwait(false);
        }

        return RedirectToAction("Consent");
    }

    /// <summary>
    /// Displays the user edit screen.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("User/Edit")]
    [HttpGet("User/UpdateCredentials")]
    [HttpGet("User/UpdateTwoFactor")]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Unauthorized();
        }
        ViewBag.IsUpdated = false;
        ViewBag.IsCreated = false;
        return await GetEditView(authenticatedUser, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the credentials.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">viewModel</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCredentials(
        UpdateResourceOwnerCredentialsViewModel viewModel,
        CancellationToken cancellationToken)
    {
        // 1. Validate the view model.
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        ViewBag.IsUpdated = false;
        viewModel.Validate(ModelState);
        if (!ModelState.IsValid)
        {
            return await GetEditView(authenticatedUser!, cancellationToken).ConfigureAwait(false);
        }

        // 2. CreateJwk a new user if he doesn't exist or update the credentials.
        //var resourceOwner = await _getUserOperation.Execute(authenticatedUser).ConfigureAwait(false);
        var subject = authenticatedUser.GetSubject();
        var updated = subject != null
                      && await _resourceOwnerRepository.SetPassword(subject, viewModel.Password!, cancellationToken)
                          .ConfigureAwait(false);
        ViewBag.IsUpdated = updated;
        return await GetEditView(authenticatedUser!, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the two factor authentication.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">viewModel</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTwoFactor(
        UpdateTwoFactorAuthenticatorViewModel? viewModel,
        CancellationToken cancellationToken)
    {
        var viewModelSelectedTwoFactorAuthType = viewModel?.SelectedTwoFactorAuthType;
        if (viewModelSelectedTwoFactorAuthType == null)
        {
            return BadRequest();
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        ViewBag.IsUpdated = false;
        ViewBag.IsCreated = false;
        await _updateUserTwoFactorAuthenticatorOperation.Execute(
                authenticatedUser.GetSubject()!,
                viewModelSelectedTwoFactorAuthType,
                cancellationToken)
            .ConfigureAwait(false);
        ViewBag.IsUpdated = true;
        return await GetEditView(authenticatedUser!, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Link an external account to the local one.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task Link(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            BadRequest();
        }

        var redirectUrl = _urlHelper.Action("LinkCallback", "User", null, Request.Scheme);
        await _authenticationService.ChallengeAsync(
                HttpContext,
                provider,
                new AuthenticationProperties { RedirectUri = redirectUrl })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Callback operation used to link an external account to the local one.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="cancellationToken">The cancellation token for the callback.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> LinkCallback(string error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogError(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error);
            return SetRedirection(
                string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error),
                "500",
                ErrorCodes.UnhandledExceptionCode);
        }

        try
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var externalClaims = await _authenticationService
                .GetAuthenticatedUser(this)
                .ConfigureAwait(false);
            if (externalClaims != null)
            {
                var option = await InnerLinkProfile(authenticatedUser.GetSubject()!, externalClaims, cancellationToken)
                    .ConfigureAwait(false);
                if (option is Option.Error e)
                {
                    return SetRedirection(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
                }
            }

            await _authenticationService.SignOutAsync(
                    HttpContext,
                    null,
                    new AuthenticationProperties())
                .ConfigureAwait(false);
            return RedirectToAction("Index", "User");
        }
        catch (Exception)
        {
            await _authenticationService.SignOutAsync(
                    HttpContext,
                    null,
                    new AuthenticationProperties())
                .ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Confirm to link the external account to this local account.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> LinkProfileConfirmation()
    {
        var externalClaims = await _authenticationService.GetAuthenticatedUser(this)
            .ConfigureAwait(false);
        if (externalClaims?.Identity == null
            || !externalClaims.Identity.IsAuthenticated
            || externalClaims.Identity is not ClaimsIdentity identity)
        {
            return RedirectToAction("Index", "User");
        }

        await SetUser().ConfigureAwait(false);
        var authenticationType = identity.AuthenticationType!;
        var viewModel = new LinkProfileConfirmationViewModel(authenticationType);
        return Ok(viewModel);
    }

    /// <summary>
    /// Force to link the external account to the local one.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ConfirmProfileLinking(CancellationToken cancellationToken)
    {
        var externalClaims = await _authenticationService.GetAuthenticatedUser(this)
            .ConfigureAwait(false);
        if (externalClaims?.Identity == null
            || !externalClaims.Identity.IsAuthenticated
            || externalClaims.Identity is not ClaimsIdentity)
        {
            return RedirectToAction("Profile", "User");
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        try
        {
            var option = await InnerLinkProfile(authenticatedUser.GetSubject()!, externalClaims, cancellationToken)
                .ConfigureAwait(false);
            if (option is Option.Error e)
            {
                return SetRedirection(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
            }
            return RedirectToAction("Index", "User");
        }
        finally
        {
            await _authenticationService.SignOutAsync(
                    HttpContext,
                    null,
                    new AuthenticationProperties())
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Unlink the external account.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Unlink(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var authenticatedUser = await SetUser().ConfigureAwait(false);
        try
        {
            var option = await UnlinkProfile(
                    authenticatedUser.GetSubject()!,
                    id,
                    authenticatedUser!.Identity!.AuthenticationType!,
                    cancellationToken)
                .ConfigureAwait(false);

            if (option is Option.Error e)
            {
                return RedirectToAction(
                    "Index",
                    "Error",
                    new { code = e.Details.Status.ToString(), message = e.Details.Detail });
            }
        }
        catch (Exception ex)
        {
            return RedirectToAction("Index", "Error", new { code = ErrorCodes.InternalError, message = ex.Message });
        }

        return await Index(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> GetEditView(
        ClaimsPrincipal authenticatedUser,
        CancellationToken cancellationToken)
    {
        var option =
            await _getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
        UpdateResourceOwnerViewModel viewModel;
        var subject = authenticatedUser.GetSubject()!;
        if (option is not Option<ResourceOwner>.Result ro)
        {
            viewModel = BuildViewModel(string.Empty, subject, authenticatedUser.Claims, false);
            return View("Edit", viewModel);
        }

        var resourceOwner = ro.Item;
        viewModel = BuildViewModel(
            resourceOwner.TwoFactorAuthentication ?? string.Empty,
            subject,
            resourceOwner.Claims,
            true);
        return View("Edit", viewModel);
    }

    private async Task<IActionResult> GetConsents(CancellationToken cancellationToken)
    {
        var authenticatedUser = await SetUser().ConfigureAwait(false);
        var consents = await _consentRepository
            .GetConsentsForGivenUser(authenticatedUser.GetSubject()!, cancellationToken)
            .ConfigureAwait(false);
        var result = new List<ConsentViewModel>();

        var scopes = (await _scopeRepository.SearchByNames(
                cancellationToken,
                consents.SelectMany(x => x.GrantedScopes).Distinct().ToArray())
            .ConfigureAwait(false)).ToDictionary(x => x.Name, x => x);
        result.AddRange(
            from consent in consents
            let scopeNames = consent.GrantedScopes
            let claims = consent.Claims
            select new ConsentViewModel
            {
                Id = consent.Id,
                ClientDisplayName = consent.ClientName,
                AllowedScopeDescriptions = scopeNames?.Any() != true
                    ? new List<string>()
                    : scopeNames.Select(g => scopes[g].Description).ToList(),
                AllowedIndividualClaims = claims ?? Array.Empty<string>(),
                //LogoUri = client?.LogoUri?.AbsoluteUri,
                PolicyUri = consent.PolicyUri?.AbsoluteUri,
                TosUri = consent.TosUri?.AbsoluteUri
            });

        return Ok(result);
    }

    private UpdateResourceOwnerViewModel BuildViewModel(
        string twoFactorAuthType,
        string subject,
        IEnumerable<Claim> claims,
        bool isLocalAccount)
    {
        var editableClaims = new Dictionary<string, string>();
        var notEditableClaims = new Dictionary<string, string>();
        foreach (var claim in claims)
        {
            if (JwtConstants.NotEditableResourceOwnerClaimNames.Contains(claim.Type))
            {
                notEditableClaims.Add(claim.Type, claim.Value);
            }
            else
            {
                editableClaims.Add(claim.Type, claim.Value);
            }
        }

        var result = new UpdateResourceOwnerViewModel(
            subject,
            editableClaims,
            notEditableClaims,
            isLocalAccount,
            twoFactorAuthType,
            _twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList());
        return result;
    }


    /// <summary>
    /// Get the profiles linked to the user account.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    private async Task<ResourceOwner?> GetUserProfile(string subject, CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(subject)
            ? null
            : await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Option> UnlinkProfile(
        string localSubject,
        string externalSubject,
        string issuer,
        CancellationToken cancellationToken)
    {
        var resourceOwner =
            await _resourceOwnerRepository.Get(localSubject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = Strings.TheRoDoesntExist,
                    Status = HttpStatusCode.InternalServerError
                });
        }

        var unlink = resourceOwner.ExternalLogins.Where(
                x => x.Subject == externalSubject && (x.Issuer == issuer || issuer == CookieNames.CookieName))
            .ToArray();
        if (unlink.Length <= 0)
        {
            return new Option.Error(new ErrorDetails { Title = ErrorCodes.InvalidRequest });
        }

        resourceOwner.ExternalLogins = resourceOwner.ExternalLogins.Remove(unlink);
        var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken)
            .ConfigureAwait(false);
        return result;

    }

    private async Task<Option> InnerLinkProfile(
        string localSubject,
        ClaimsPrincipal externalPrincipal,
        CancellationToken cancellationToken)
    {
        var resourceOwner =
            await _resourceOwnerRepository.Get(localSubject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = Strings.TheRoDoesntExist,
                    Status = HttpStatusCode.InternalServerError
                });
        }

        var issuer = externalPrincipal.Identity!.AuthenticationType;
        var externalSubject = externalPrincipal.GetSubject();
        if (resourceOwner.ExternalLogins.Any(x => x.Subject == externalSubject && x.Issuer == issuer))
        {
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = Strings.TheRoDoesntExist,
                    Status = HttpStatusCode.InternalServerError
                });
        }

        var newClaims = externalPrincipal.Claims.ToOpenidClaims()
            .Where(c => resourceOwner.Claims.All(x => x.Type != c.Type));
        resourceOwner.Claims = resourceOwner.Claims.Add(newClaims);

        resourceOwner.ExternalLogins = resourceOwner.ExternalLogins.Concat(
                new[]
                {
                    new ExternalAccountLink
                    {
                        Issuer = issuer!,
                        Subject = externalSubject!,
                        ExternalClaims = externalPrincipal.Claims.ToArray()
                    }
                })
            .ToArray();
        await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        return new Option.Success();

    }
}