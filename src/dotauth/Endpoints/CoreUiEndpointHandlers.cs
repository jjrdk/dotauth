namespace DotAuth.Endpoints;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Events.Openid;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.ViewModels;
using DotAuth.WebSite.Consent.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

internal static class CoreUiEndpointHandlers
{
    private const string HomeView = "/Views/Home/Index.cshtml";
    private const string ErrorView = "/Views/Error/Index.cshtml";
    private const string FormView = "/Views/Form/Index.cshtml";
    private const string ConsentView = "/Views/Consent/Index.cshtml";

    internal static async Task<IResult> GetHome(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        RuntimeSettings settings,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.HomeController");
        await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (settings.RedirectToLogin)
        {
            logger.LogDebug("Redirecting to login page");
            return Results.Redirect("/authenticate");
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, HomeView, new object());
    }

    internal static IResult GetError(HttpContext httpContext, [FromQuery] string code, [FromQuery] string? title, [FromQuery] string? message)
    {
        if (!Enum.TryParse<System.Net.HttpStatusCode>(code, out var statusCode))
        {
            statusCode = System.Net.HttpStatusCode.BadRequest;
        }

        title ??= statusCode.ToString();
        message ??= title;

        if (!UiEndpointHelpers.WantsHtml(httpContext.Request))
        {
            return Results.Json(
                UiEndpointHelpers.CreateErrorDetails(title, message, statusCode),
                statusCode: (int)statusCode);
        }

        var viewModel = new ErrorViewModel
        {
            Code = (int)statusCode,
            Title = title,
            Message = message
        };
        return UiEndpointHelpers.ViewOrJson(httpContext, ErrorView, viewModel, (int)statusCode);
    }

    internal static IResult GetError400(HttpContext httpContext) =>
        GetError(httpContext, "400", Strings.Badrequest, Strings.Http400);

    internal static IResult GetError401(HttpContext httpContext) =>
        GetError(httpContext, "401", Strings.Unauthorized, Strings.Http401);

    internal static IResult GetError404(HttpContext httpContext) =>
        GetError(httpContext, "404", Strings.NotFound, Strings.Http404);

    internal static IResult GetError500(HttpContext httpContext) =>
        GetError(httpContext, "500", Strings.InternalServerError, Strings.Http500);

    internal static IResult GetForm(HttpContext httpContext)
    {
        var queryStringValue = httpContext.Request.QueryString.Value ?? string.Empty;
        var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryStringValue);
        var viewModel = new FormViewModel();
        if (queryString.ContainsKey(StandardAuthorizationResponseNames.AccessTokenName))
        {
            viewModel.AccessToken = queryString[StandardAuthorizationResponseNames.AccessTokenName];
        }

        if (queryString.ContainsKey(StandardAuthorizationResponseNames.AuthorizationCodeName))
        {
            viewModel.AuthorizationCode = queryString[StandardAuthorizationResponseNames.AuthorizationCodeName];
        }

        if (queryString.ContainsKey(StandardAuthorizationResponseNames.IdTokenName))
        {
            viewModel.IdToken = queryString[StandardAuthorizationResponseNames.IdTokenName];
        }

        if (queryString.ContainsKey(StandardAuthorizationResponseNames.StateName))
        {
            viewModel.State = queryString[StandardAuthorizationResponseNames.StateName];
        }

        if (queryString.ContainsKey("redirect_uri"))
        {
            viewModel.RedirectUri = queryString["redirect_uri"];
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, FormView, viewModel);
    }

    internal static async Task<IResult> GetConsent(
        HttpContext httpContext,
        string code,
        CancellationToken cancellationToken,
        IScopeRepository scopeRepository,
        IClientStore clientStore,
        IConsentRepository consentRepository,
        IDataProtectionProvider dataProtectionProvider,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IAuthorizationCodeStore authorizationCodeStore,
        IAuthenticationService authenticationService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.ConsentController");
        var dataProtector = dataProtectionProvider.CreateProtector("Request");
        var request = dataProtector.Unprotect<AuthorizationRequest>(code);
        if (request.client_id == null)
        {
            return Results.BadRequest();
        }

        var displayConsent = new DisplayConsentAction(
            scopeRepository,
            clientStore,
            consentRepository,
            authorizationCodeStore,
            tokenStore,
            jwksStore,
            eventPublisher,
            logger);
        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.CookieName).ConfigureAwait(false) ?? new ClaimsPrincipal();
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await displayConsent.Execute(request.ToParameter(), authenticatedUser, issuerName, cancellationToken).ConfigureAwait(false);

        var result = actionResult.EndpointResult.CreateRedirectionFromActionResult(request, logger);
        if (result != null)
        {
            return UiEndpointHelpers.ToRedirectResult(httpContext, result);
        }

        var client = await clientStore.GetById(request.client_id, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return Results.BadRequest();
        }

        var viewModel = new ConsentViewModel
        {
            ClientDisplayName = client.ClientName,
            AllowedScopeDescriptions = actionResult.Scopes.Select(s => s.Description).ToList(),
            AllowedIndividualClaims = actionResult.AllowedClaims,
            LogoUri = client.LogoUri?.AbsoluteUri,
            PolicyUri = client.PolicyUri?.AbsoluteUri,
            TosUri = client.TosUri?.AbsoluteUri,
            Code = code
        };
        return UiEndpointHelpers.ViewOrJson(httpContext, ConsentView, viewModel);
    }

    internal static async Task<IResult> ConfirmConsent(
        HttpContext httpContext,
        string code,
        CancellationToken cancellationToken,
        IDataProtectionProvider dataProtectionProvider,
        IAuthenticationService authenticationService,
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IConsentRepository consentRepository,
        IClientStore clientStore,
        IScopeRepository scopeRepository,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.ConsentController");
        var dataProtector = dataProtectionProvider.CreateProtector("Request");
        var request = dataProtector.Unprotect<AuthorizationRequest>(code);
        var parameter = request.ToParameter();
        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.CookieName).ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Results.Unauthorized();
        }

        var confirmConsent = new ConfirmConsentAction(
            authorizationCodeStore,
            tokenStore,
            consentRepository,
            clientStore,
            scopeRepository,
            jwksStore,
            eventPublisher,
            logger);
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await confirmConsent.Execute(parameter, authenticatedUser, issuerName, cancellationToken).ConfigureAwait(false);
        var subject = authenticatedUser.GetSubject();
        await eventPublisher.Publish(new ConsentAccepted(Id.Create(), subject!, request.client_id!, request.scope!, DateTimeOffset.UtcNow)).ConfigureAwait(false);
        var redirect = actionResult.CreateRedirectionFromActionResult(request, logger)!;
        return UiEndpointHelpers.ToRedirectResult(httpContext, redirect);
    }

    internal static async Task<IResult> CancelConsent(
        string code,
        IDataProtectionProvider dataProtectionProvider,
        IEventPublisher eventPublisher)
    {
        var dataProtector = dataProtectionProvider.CreateProtector("Request");
        var request = dataProtector.Unprotect<AuthorizationRequest>(code);
        if (request.redirect_uri == null)
        {
            return Results.BadRequest();
        }

        await eventPublisher.Publish(
                new ConsentRejected(
                    Id.Create(),
                    request.client_id ?? string.Empty,
                    request.scope == null ? [] : request.scope.Trim().Split(' '),
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        return Results.Redirect(request.redirect_uri.AbsoluteUri);
    }
}


