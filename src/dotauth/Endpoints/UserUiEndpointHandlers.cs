namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.ViewModels;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class UserUiEndpointHandlers
{
    private const string IndexView = "/Views/User/Index.cshtml";
    private const string ConsentView = "/Views/User/Consent.cshtml";
    private const string EditView = "/Views/User/Edit.cshtml";
    private const string LinkProfileConfirmationView = "/Views/User/LinkProfileConfirmation.cshtml";

    internal static async Task<IResult> GetIndex(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        CancellationToken cancellationToken)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Results.Redirect("/");
        }

        var subject = authenticatedUser.GetSubject();
        var ro = subject == null ? null : await resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
        if (ro == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InternalError,
                Detail = Strings.TheRoDoesntExist
            });
        }

        var authenticationSchemes = (await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false))
            .Where(a => !string.IsNullOrWhiteSpace(a.DisplayName));
        var viewModel = new ProfileViewModel(ro.Claims);
        foreach (var profile in ro.ExternalLogins)
        {
            viewModel.LinkedIdentityProviders.Add(new IdentityProviderViewModel(profile.Issuer, profile.Subject));
        }

        var actualScheme = authenticatedUser.Identity?.AuthenticationType;
        viewModel.UnlinkedIdentityProviders = authenticationSchemes
            .Where(a => a.DisplayName != null
                        && !a.DisplayName.StartsWith('_')
                        && !ro.ExternalLogins.Any(p => p.Issuer == a.Name && a.Name != actualScheme))
            .Select(p => new IdentityProviderViewModel(p.Name))
            .ToList();

        return UiEndpointHelpers.ViewOrJson(httpContext, IndexView, viewModel);
    }

    internal static async Task<IResult> GetConsent(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        return await GetConsents(httpContext, authenticationService, consentRepository, scopeRepository, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<IResult> PostConsent(
        HttpContext httpContext,
        string id,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var removed = await consentRepository.Delete(new Consent { Id = id }, cancellationToken).ConfigureAwait(false);
        if (!removed)
        {
            var current = await GetConsents(httpContext, authenticationService, consentRepository, scopeRepository, cancellationToken).ConfigureAwait(false);
            return current;
        }

        return Results.Redirect("/user/consent");
    }

    internal static async Task<IResult> Edit(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Results.Unauthorized();
        }

        return await GetEditView(
                httpContext,
                authenticatedUser!,
                resourceOwnerRepository,
                twoFactorAuthenticationHandler,
                loggerFactory,
                cancellationToken,
                isUpdated: false,
                isCreated: false)
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> UpdateCredentials(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        CancellationToken cancellationToken,
        ILoggerFactory loggerFactory)
    {
        var viewModel = await EndpointHandlerHelpers.BindFromFormAsync<UpdateResourceOwnerCredentialsViewModel>(httpContext.Request).ConfigureAwait(false);
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.UserController");
        var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        viewModel.Validate(modelState);
        if (!modelState.IsValid)
        {
            return await GetEditView(
                    httpContext,
                    authenticatedUser!,
                    resourceOwnerRepository,
                    twoFactorAuthenticationHandler,
                    loggerFactory,
                    cancellationToken,
                    isUpdated: false,
                    isCreated: false,
                    modelState: modelState)
                .ConfigureAwait(false);
        }

        var subject = authenticatedUser!.GetSubject();
        var updated = subject != null && await resourceOwnerRepository.SetPassword(subject, viewModel.Password!, cancellationToken).ConfigureAwait(false);
        logger.LogDebug("User credentials updated: {Updated}", updated);
        return await GetEditView(
                httpContext,
                authenticatedUser!,
                resourceOwnerRepository,
                twoFactorAuthenticationHandler,
                loggerFactory,
                cancellationToken,
                isUpdated: updated,
                isCreated: false)
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> UpdateTwoFactor(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var viewModel = await EndpointHandlerHelpers.BindFromFormAsync<UpdateTwoFactorAuthenticatorViewModel>(httpContext.Request).ConfigureAwait(false);
        if (viewModel.SelectedTwoFactorAuthType == null)
        {
            return Results.BadRequest();
        }

        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var operation = new UpdateUserTwoFactorAuthenticatorOperation(resourceOwnerRepository, loggerFactory.CreateLogger("DotAuth.Controllers.UserController"));
        await operation.Execute(authenticatedUser!.GetSubject()!, viewModel.SelectedTwoFactorAuthType, cancellationToken).ConfigureAwait(false);
        return await GetEditView(
                httpContext,
                authenticatedUser!,
                resourceOwnerRepository,
                twoFactorAuthenticationHandler,
                loggerFactory,
                cancellationToken,
                isUpdated: true,
                isCreated: false)
            .ConfigureAwait(false);
    }

    internal static async Task Link(
        HttpContext httpContext,
        string provider,
        IAuthenticationService authenticationService,
        LinkGenerator linkGenerator)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var redirectUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}/user/linkcallback";
        await authenticationService.ChallengeAsync(httpContext, provider, new AuthenticationProperties { RedirectUri = redirectUrl }).ConfigureAwait(false);
    }

    internal static async Task<IResult> LinkCallback(
        HttpContext httpContext,
        string error,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.UserController");
        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogError("{Error}", Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate);
            return UiEndpointHelpers.RedirectToError(
                string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error),
                "500",
                ErrorCodes.UnhandledExceptionCode);
        }

        try
        {
            var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
            var externalClaims = await authenticationService.GetAuthenticatedUser(httpContext).ConfigureAwait(false);
            if (externalClaims != null)
            {
                var option = await InnerLinkProfile(authenticatedUser!.GetSubject()!, externalClaims, resourceOwnerRepository, cancellationToken).ConfigureAwait(false);
                if (option is Option.Error e)
                {
                    return UiEndpointHelpers.RedirectToError(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
                }
            }

            await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
            return Results.Redirect("/user");
        }
        catch
        {
            await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
            throw;
        }
    }

    internal static async Task<IResult> LinkProfileConfirmation(
        HttpContext httpContext,
        IAuthenticationService authenticationService)
    {
        var externalClaims = await authenticationService.GetAuthenticatedUser(httpContext).ConfigureAwait(false);
        if (externalClaims?.Identity is not ({ IsAuthenticated: true } and ClaimsIdentity identity))
        {
            return Results.Redirect("/user");
        }

        await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var viewModel = new LinkProfileConfirmationViewModel(identity.AuthenticationType!);
        return UiEndpointHelpers.ViewOrJson(httpContext, LinkProfileConfirmationView, viewModel);
    }

    internal static async Task<IResult> ConfirmProfileLinking(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var externalClaims = await authenticationService.GetAuthenticatedUser(httpContext).ConfigureAwait(false);
        if (externalClaims?.Identity is not ({ IsAuthenticated: true } and ClaimsIdentity))
        {
            return Results.Redirect("/user/profile");
        }

        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        try
        {
            var option = await InnerLinkProfile(authenticatedUser!.GetSubject()!, externalClaims, resourceOwnerRepository, cancellationToken).ConfigureAwait(false);
            if (option is Option.Error e)
            {
                return UiEndpointHelpers.RedirectToError(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
            }

            return Results.Redirect("/user");
        }
        finally
        {
            await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
        }
    }

    internal static async Task<IResult> Unlink(
        HttpContext httpContext,
        string id,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Results.BadRequest();
        }

        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        try
        {
            var option = await UnlinkProfile(
                    authenticatedUser!.GetSubject()!,
                    id,
                    authenticatedUser?.Identity?.AuthenticationType ?? CookieNames.CookieName,
                    resourceOwnerRepository,
                    cancellationToken)
                .ConfigureAwait(false);

            if (option is Option.Error e)
            {
                return Results.Redirect($"/error?code={e.Details.Status}&message={Uri.EscapeDataString(e.Details.Detail)}");
            }
        }
        catch (Exception ex)
        {
            return Results.Redirect($"/error?code={ErrorCodes.InternalError}&message={Uri.EscapeDataString(ex.Message)}");
        }

        return await GetIndex(httpContext, authenticationService, resourceOwnerRepository, httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>(), httpContext.RequestServices.GetRequiredService<ITwoFactorAuthenticationHandler>(), cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IResult> GetEditView(
        HttpContext httpContext,
        ClaimsPrincipal authenticatedUser,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken,
        bool isUpdated,
        bool isCreated,
        Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary? modelState = null)
    {
        var option = await new GetUserOperation(resourceOwnerRepository, loggerFactory.CreateLogger("DotAuth.Controllers.UserController"))
            .Execute(authenticatedUser, cancellationToken)
            .ConfigureAwait(false);
        var subject = authenticatedUser.GetSubject()!;
        UpdateResourceOwnerViewModel viewModel;
        if (option is not Option<ResourceOwner>.Result ro)
        {
            viewModel = BuildViewModel(string.Empty, subject, authenticatedUser.Claims, false, twoFactorAuthenticationHandler);
            return UiEndpointHelpers.ViewOrJson(
                httpContext,
                EditView,
                viewModel,
                viewData: UiEndpointHelpers.CreateViewData(("IsUpdated", isUpdated), ("IsCreated", isCreated)),
                modelState: modelState);
        }

        var resourceOwner = ro.Item;
        viewModel = BuildViewModel(resourceOwner.TwoFactorAuthentication ?? string.Empty, subject, resourceOwner.Claims, true, twoFactorAuthenticationHandler);
        return UiEndpointHelpers.ViewOrJson(
            httpContext,
            EditView,
            viewModel,
            viewData: UiEndpointHelpers.CreateViewData(("IsUpdated", isUpdated), ("IsCreated", isCreated)),
            modelState: modelState);
    }

    private static async Task<IResult> GetConsents(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var consents = await consentRepository.GetConsentsForGivenUser(authenticatedUser!.GetSubject()!, cancellationToken).ConfigureAwait(false);
        var scopes = (await scopeRepository.SearchByNames(cancellationToken, consents.SelectMany(x => x.GrantedScopes).Distinct().ToArray()).ConfigureAwait(false))
            .ToDictionary(x => x.Name, x => x);
        var result = new List<ConsentViewModel>();
        result.AddRange(
            from consent in consents
            let scopeNames = consent.GrantedScopes
            let claims = consent.Claims
            select new ConsentViewModel
            {
                Id = consent.Id,
                ClientDisplayName = consent.ClientName,
                AllowedScopeDescriptions = scopeNames?.Any() != true ? [] : scopeNames.Select(g => scopes[g].Description).ToList(),
                AllowedIndividualClaims = claims ?? [],
                PolicyUri = consent.PolicyUri?.AbsoluteUri,
                TosUri = consent.TosUri?.AbsoluteUri
            });
        return UiEndpointHelpers.ViewOrJson(httpContext, ConsentView, result);
    }

    private static UpdateResourceOwnerViewModel BuildViewModel(
        string twoFactorAuthType,
        string subject,
        IEnumerable<Claim> claims,
        bool isLocalAccount,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler)
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

        return new UpdateResourceOwnerViewModel(
            subject,
            editableClaims,
            notEditableClaims,
            isLocalAccount,
            twoFactorAuthType,
            twoFactorAuthenticationHandler.GetAll().Select(s => s.Name).ToList());
    }

    private static async Task<Option> UnlinkProfile(
        string localSubject,
        string externalSubject,
        string issuer,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var resourceOwner = await resourceOwnerRepository.Get(localSubject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new Option.Error(new ErrorDetails
            {
                Title = ErrorCodes.InternalError,
                Detail = Strings.TheRoDoesntExist,
                Status = HttpStatusCode.InternalServerError
            });
        }

        var unlink = resourceOwner.ExternalLogins.Where(x => x.Subject == externalSubject && (x.Issuer == issuer || issuer == CookieNames.CookieName)).ToArray();
        if (unlink.Length <= 0)
        {
            return new Option.Error(new ErrorDetails { Title = ErrorCodes.InvalidRequest, Detail = ErrorCodes.InvalidRequest });
        }

        resourceOwner.ExternalLogins = resourceOwner.ExternalLogins.Remove(unlink);
        return await resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Option> InnerLinkProfile(
        string localSubject,
        ClaimsPrincipal externalPrincipal,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var resourceOwner = await resourceOwnerRepository.Get(localSubject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new Option.Error(new ErrorDetails
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
            return new Option.Error(new ErrorDetails
            {
                Title = ErrorCodes.InternalError,
                Detail = Strings.TheRoDoesntExist,
                Status = HttpStatusCode.InternalServerError
            });
        }

        var newClaims = externalPrincipal.Claims.ToOpenidClaims().Where(c => resourceOwner.Claims.All(x => x.Type != c.Type));
        resourceOwner.Claims = resourceOwner.Claims.Add(newClaims);
        resourceOwner.ExternalLogins = resourceOwner.ExternalLogins.Concat([
            new ExternalAccountLink
            {
                Issuer = issuer!,
                Subject = externalSubject!,
                ExternalClaims = externalPrincipal.Claims.ToArray()
            }
        ]).ToArray();
        await resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        return new Option.Success();
    }
}



