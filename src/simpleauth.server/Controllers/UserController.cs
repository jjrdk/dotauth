namespace SimpleAuth.Server.Controllers
{
    using Errors;
    using Exceptions;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Server;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Extensions;
    using SimpleAuth.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Translation;
    using ViewModels;
    using WebSite.User.Actions;

    [Area("UserManagement")]
    [Authorize("Connected")]
    public class UserController : BaseController
    {
        private const string DefaultLanguage = "en";
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IUpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;
        private readonly IUpdateUserCredentialsOperation _updateUserCredentialsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        // private readonly IProfileActions _profileActions;
        private readonly ITranslationManager _translationManager;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IUrlHelper _urlHelper;
        private readonly ITwoFactorAuthenticationHandler _twoFactorAuthenticationHandler;

        public UserController(
            IResourceOwnerRepository resourceOwnerRepository,
            IProfileRepository profileRepository,
            IGetUserOperation getUserOperation,
            IGetConsentsOperation getConsentsOperation,
            IUpdateUserTwoFactorAuthenticatorOperation updateUserTwoFactorAuthenticatorOperation,
            IUpdateUserCredentialsOperation updateUserCredentialsOperation,
            IRemoveConsentOperation removeConsentOperation,
            ITranslationManager translationManager,
            IAuthenticationService authenticationService,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler) : base(authenticationService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _profileRepository = profileRepository;
            _getUserOperation = getUserOperation;
            _getConsentsOperation = getConsentsOperation;
            _updateUserTwoFactorAuthenticatorOperation = updateUserTwoFactorAuthenticatorOperation;
            _updateUserCredentialsOperation = updateUserCredentialsOperation;
            _removeConsentOperation = removeConsentOperation;
            // _profileActions = profileActions;
            _translationManager = translationManager;
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _twoFactorAuthenticationHandler = twoFactorAuthenticationHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetUser().ConfigureAwait(false);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Consent()
        {
            await SetUser().ConfigureAwait(false);
            return await GetConsents().ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<IActionResult> Consent(string id)
        {
            if (!await _removeConsentOperation.Execute(id).ConfigureAwait(false))
            {
                ViewBag.ErrorMessage = "the consent cannot be deleted";
                return await GetConsents().ConfigureAwait(false);
            }

            return RedirectToAction("Consent");
        }

        [HttpGet("User/Edit")]
        [HttpGet("User/UpdateCredentials")]
        [HttpGet("User/UpdateTwoFactor")]
        public async Task<IActionResult> Edit()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCredentials(UpdateResourceOwnerCredentialsViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            // 1. Validate the view model.
            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            viewModel.Validate(ModelState);
            if (!ModelState.IsValid)
            {
                return await GetEditView(authenticatedUser).ConfigureAwait(false);
            }

            // 2. Create a new user if he doesn't exist or update the credentials.
            //var resourceOwner = await _getUserOperation.Execute(authenticatedUser).ConfigureAwait(false);
            var subject = authenticatedUser.GetSubject();
            await _updateUserCredentialsOperation.Execute(subject, viewModel.Password).ConfigureAwait(false);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTwoFactor(UpdateTwoFactorAuthenticatorViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            await TranslateUserEditView(DefaultLanguage).ConfigureAwait(false);
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            ViewBag.IsUpdated = false;
            ViewBag.IsCreated = false;
            await _updateUserTwoFactorAuthenticatorOperation
                .Execute(authenticatedUser.GetSubject(), viewModel.SelectedTwoFactorAuthType)
                .ConfigureAwait(false);
            ViewBag.IsUpdated = true;
            return await GetEditView(authenticatedUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Display the profiles linked to the user account.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var actualScheme = authenticatedUser.Identity.AuthenticationType;
            var profiles = await GetUserProfiles(authenticatedUser.GetSubject()).ConfigureAwait(false);
            var authenticationSchemes =
                (await _authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(a =>
                    !string.IsNullOrWhiteSpace(a.DisplayName));
            var viewModel = new ProfileViewModel();
            if (profiles != null && profiles.Any())
            {
                foreach (var profile in profiles)
                {
                    var record = new IdentityProviderViewModel(profile.Issuer, profile.Subject);
                    viewModel.LinkedIdentityProviders.Add(record);
                }
            }

            viewModel.UnlinkedIdentityProviders = authenticationSchemes
                .Where(a => profiles != null && !profiles.Any(p => p.Issuer == a.Name && a.Name != actualScheme))
                .Select(p => new IdentityProviderViewModel(p.Name))
                .ToList();
            return View("Profile", viewModel);
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
                throw new ArgumentNullException(nameof(provider));
            }

            var redirectUrl = _urlHelper.AbsoluteAction("LinkCallback", "User");
            await _authenticationService.ChallengeAsync(HttpContext,
                    provider,
                    new AuthenticationProperties()
                    {
                        RedirectUri = redirectUrl
                    })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Callback operation used to link an external account to the local one.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LinkCallback(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new SimpleAuthException(ErrorCodes.UnhandledExceptionCode,
                    string.Format(ErrorDescriptions.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            }

            try
            {
                var authenticatedUser = await SetUser().ConfigureAwait(false);
                var externalClaims = await _authenticationService
                    .GetAuthenticatedUser(this, HostConstants.CookieNames.ExternalCookieName)
                    .ConfigureAwait(false);
                var resourceOwner = await InnerLinkProfile(
                        authenticatedUser.GetSubject(),
                        externalClaims.GetSubject(),
                        externalClaims.Identity.AuthenticationType,
                        false)
                    .ConfigureAwait(false);
                await _authenticationService
                    .SignOutAsync(HttpContext,
                        HostConstants.CookieNames.ExternalCookieName,
                        new AuthenticationProperties())
                    .ConfigureAwait(false);
                return RedirectToAction("Profile", "User");
            }
            catch (ProfileAssignedAnotherAccountException)
            {
                return RedirectToAction("LinkProfileConfirmation");
            }
            catch (Exception)
            {
                await _authenticationService
                    .SignOutAsync(HttpContext,
                        HostConstants.CookieNames.ExternalCookieName,
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
            var externalClaims = await _authenticationService
                .GetAuthenticatedUser(this, HostConstants.CookieNames.ExternalCookieName)
                .ConfigureAwait(false);
            if (externalClaims?.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User");
            }

            await SetUser().ConfigureAwait(false);
            var authenticationType = ((ClaimsIdentity)externalClaims.Identity).AuthenticationType;
            var viewModel = new LinkProfileConfirmationViewModel(authenticationType);
            return View(viewModel);
        }

        /// <summary>
        /// Force to link the external account to the local one.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmProfileLinking()
        {
            var externalClaims = await _authenticationService
                .GetAuthenticatedUser(this, HostConstants.CookieNames.ExternalCookieName)
                .ConfigureAwait(false);
            if (externalClaims?.Identity == null ||
                !externalClaims.Identity.IsAuthenticated ||
                !(externalClaims.Identity is ClaimsIdentity))
            {
                return RedirectToAction("Profile", "User");
            }

            var authenticatedUser = await SetUser().ConfigureAwait(false);
            try
            {
                await InnerLinkProfile(
                        authenticatedUser.GetSubject(),
                        externalClaims.GetSubject(),
                        externalClaims.Identity.AuthenticationType,
                        true)
                    .ConfigureAwait(false);
                return RedirectToAction("Profile", "User");
            }
            finally
            {
                await _authenticationService
                    .SignOutAsync(HttpContext,
                        HostConstants.CookieNames.ExternalCookieName,
                        new AuthenticationProperties())
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Unlink the external account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Unlink(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var authenticatedUser = await SetUser().ConfigureAwait(false);
            try
            {
                await UnlinkProfile(
                        authenticatedUser.GetSubject(),
                        id)
                    .ConfigureAwait(false);
            }
            catch (SimpleAuthException ex)
            {
                return RedirectToAction("Index", "Error", new { code = ex.Code, message = ex.Message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index",
                    "Error",
                    new { code = ErrorCodes.InternalError, message = ex.Message });
            }

            return await Profile().ConfigureAwait(false);
        }

        private async Task<IActionResult> GetEditView(ClaimsPrincipal authenticatedUser)
        {
            var resourceOwner = await _getUserOperation.Execute(authenticatedUser).ConfigureAwait(false);
            UpdateResourceOwnerViewModel viewModel = null;
            if (resourceOwner == null)
            {
                viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication,
                    authenticatedUser.GetSubject(),
                    authenticatedUser.Claims,
                    false);
                return View("Edit", viewModel);
            }

            viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication,
                authenticatedUser.GetSubject(),
                resourceOwner.Claims,
                true);
            viewModel.IsLocalAccount = true;
            return View("Edit", viewModel);
        }

        private async Task<IActionResult> GetConsents()
        {
            var authenticatedUser = await SetUser().ConfigureAwait(false);
            var consents = await _getConsentsOperation.Execute(authenticatedUser).ConfigureAwait(false);
            var result = new List<ConsentViewModel>();
            if (consents != null)
            {
                result.AddRange(from consent in consents
                                let client = consent.Client
                                let scopes = consent.GrantedScopes
                                let claims = consent.Claims
                                select new ConsentViewModel
                                {
                                    Id = consent.Id,
                                    ClientDisplayName = client == null ? string.Empty : client.ClientName,
                                    AllowedScopeDescriptions = scopes?.Any() != true
                                        ? new List<string>()
                                        : scopes.Select(g => g.Description).ToList(),
                                    AllowedIndividualClaims = claims ?? new List<string>(),
                                    LogoUri = client?.LogoUri?.AbsoluteUri,
                                    PolicyUri = client?.PolicyUri?.AbsoluteUri,
                                    TosUri = client?.TosUri?.AbsoluteUri
                                });
            }

            return View(result);
        }

        private async Task TranslateUserEditView(string uiLocales)
        {
            var translations = await _translationManager.GetTranslationsAsync(uiLocales,
                    new List<string>
                    {
                        CoreConstants.StandardTranslationCodes.LoginCode,
                        CoreConstants.StandardTranslationCodes.EditResourceOwner,
                        CoreConstants.StandardTranslationCodes.NameCode,
                        CoreConstants.StandardTranslationCodes.YourName,
                        CoreConstants.StandardTranslationCodes.PasswordCode,
                        CoreConstants.StandardTranslationCodes.YourPassword,
                        CoreConstants.StandardTranslationCodes.Email,
                        CoreConstants.StandardTranslationCodes.YourEmail,
                        CoreConstants.StandardTranslationCodes.ConfirmCode,
                        CoreConstants.StandardTranslationCodes.TwoAuthenticationFactor,
                        CoreConstants.StandardTranslationCodes.UserIsUpdated,
                        CoreConstants.StandardTranslationCodes.Phone,
                        CoreConstants.StandardTranslationCodes.HashedPassword,
                        CoreConstants.StandardTranslationCodes.CreateResourceOwner,
                        CoreConstants.StandardTranslationCodes.Credentials,
                        CoreConstants.StandardTranslationCodes.RepeatPassword,
                        CoreConstants.StandardTranslationCodes.Claims,
                        CoreConstants.StandardTranslationCodes.UserIsCreated,
                        CoreConstants.StandardTranslationCodes.TwoFactor,
                        CoreConstants.StandardTranslationCodes.NoTwoFactorAuthenticator,
                        CoreConstants.StandardTranslationCodes.NoTwoFactorAuthenticatorSelected
                    })
                .ConfigureAwait(false);

            ViewBag.Translations = translations;
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

            var result = new UpdateResourceOwnerViewModel(subject, editableClaims, notEditableClaims, isLocalAccount)
            {
                SelectedTwoFactorAuthType = twoFactorAuthType,
                TwoFactorAuthTypes = _twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList()
            };
            return result;
        }


        /// <summary>
        /// Get the profiles linked to the user account.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ResourceOwnerProfile>> GetUserProfiles(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    Errors.ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, subject));
            }

            return await _profileRepository.Search(
                    new SearchProfileParameter
                    {
                        ResourceOwnerIds = new[] { subject }
                    })
                .ConfigureAwait(false);
        }

        private async Task<bool> UnlinkProfile(string localSubject, string externalSubject)
        {
            if (string.IsNullOrWhiteSpace(localSubject))
            {
                throw new ArgumentNullException(nameof(localSubject));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(localSubject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    Errors.ErrorCodes.InternalError,
                    string.Format(Errors.ErrorDescriptions.TheResourceOwnerDoesntExist, localSubject));
            }

            var profile = await _profileRepository.Get(externalSubject).ConfigureAwait(false);
            if (profile == null || profile.ResourceOwnerId != localSubject)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NotAuthorizedToRemoveTheProfile);
            }

            if (profile.Subject == localSubject)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheExternalAccountAccountCannotBeUnlinked);
            }

            return await _profileRepository.Remove(new[] { externalSubject }).ConfigureAwait(false);
        }

        private async Task<bool> InnerLinkProfile(string localSubject, string externalSubject, string issuer, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(localSubject))
            {
                throw new ArgumentNullException(nameof(localSubject));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(localSubject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    Errors.ErrorCodes.InternalError,
                    string.Format(Errors.ErrorDescriptions.TheResourceOwnerDoesntExist, localSubject));
            }

            var profile = await _profileRepository.Get(externalSubject).ConfigureAwait(false);
            if (profile != null && profile.ResourceOwnerId != localSubject)
            {
                if (!force)
                {
                    throw new ProfileAssignedAnotherAccountException();
                }

                await _profileRepository.Remove(new[] { externalSubject }).ConfigureAwait(false);
                if (profile.ResourceOwnerId == profile.Subject)
                {
                    await _resourceOwnerRepository.Delete(profile.ResourceOwnerId).ConfigureAwait(false);
                }

                profile = null;
            }

            if (profile != null)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheProfileAlreadyLinked);
            }

            return await _profileRepository.Add(new[]
            {
                new ResourceOwnerProfile
                {
                    ResourceOwnerId = localSubject,
                    Subject = externalSubject,
                    Issuer = issuer,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow
                }
            }).ConfigureAwait(false);
        }
    }
}
