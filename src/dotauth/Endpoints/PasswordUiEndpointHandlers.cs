namespace DotAuth.Endpoints;

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
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Logging;
using DotAuth.Shared.Events.Openid;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Parameters;
using DotAuth.ViewModels;
using DotAuth.WebSite.Authenticate;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class PasswordUiEndpointHandlers
{
    private const string AuthenticateIndexView = "/Views/Authenticate/Index.cshtml";
    private const string AuthenticateOpenIdView = "/Views/Authenticate/OpenId.cshtml";
    private const string AuthenticateSendCodeView = "/Views/Authenticate/SendCode.cshtml";
    private const string ExternalAuthenticateCookieName = "ExternalAuth-{0}";
    private const string InvalidCredentials = "invalid_credentials";

    internal static async Task<IResult> GetAuthenticateIndex(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var hasReturnUrl = httpContext.Request.Query.TryGetValue("ReturnUrl", out var returnUrl);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            var viewModel = new AuthorizeViewModel { ReturnUrl = returnUrl };
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateIndexView, viewModel);
        }

        return Results.Redirect(hasReturnUrl ? returnUrl.ToString() : "/User");
    }

    internal static async Task<IResult> PostLocalLogin(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        IEventPublisher eventPublisher,
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var authorizeViewModel = await EndpointHandlerHelpers.BindFromFormAsync<LocalAuthenticationViewModel>(httpContext.Request).ConfigureAwait(false);
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (authenticatedUser?.Identity is { IsAuthenticated: true })
        {
            return Results.Redirect("/User");
        }

        var modelState = new ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(authorizeViewModel.Login))
        {
            modelState.AddModelError("Login", "the user name is required");
        }
        if (string.IsNullOrWhiteSpace(authorizeViewModel.Password))
        {
            modelState.AddModelError("Password", "the password is required");
        }

        if (!modelState.IsValid)
        {
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateIndexView, viewModel, modelState: modelState);
        }

        try
        {
            var services = resourceOwnerServices.ToArray();
            var resourceOwner = await services.Authenticate(authorizeViewModel.Login!, authorizeViewModel.Password!, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                logger.LogError("{Error}", Strings.TheResourceOwnerCredentialsAreNotCorrect);
                var viewModel = new AuthorizeViewModel
                {
                    Password = authorizeViewModel.Password,
                    UserName = authorizeViewModel.Login
                };
                await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
                return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateIndexView, viewModel, modelState: modelState);
            }

            resourceOwner.Claims = resourceOwner.Claims.Add(
                new Claim(
                    ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer));
            var subject = resourceOwner.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;
            if (string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
            {
                await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, resourceOwner.Claims, Id.Create()).ConfigureAwait(false);
                return Results.Redirect(!string.IsNullOrWhiteSpace(authorizeViewModel.ReturnUrl) ? authorizeViewModel.ReturnUrl : "/User");
            }

            await UiEndpointHelpers.SetTwoFactorCookieAsync(httpContext, authenticationService, resourceOwner.Claims).ConfigureAwait(false);
            var generateAndSendCode = new GenerateAndSendCodeAction(
                httpContext.RequestServices.GetRequiredService<IResourceOwnerRepository>(),
                httpContext.RequestServices.GetRequiredService<IConfirmationCodeStore>(),
                httpContext.RequestServices.GetRequiredService<ITwoFactorAuthenticationHandler>(),
                logger);
            try
            {
                await generateAndSendCode.Send(subject, cancellationToken).ConfigureAwait(false);
                return Results.Redirect("/Authenticate/SendCode");
            }
            catch (ClaimRequiredException cre)
            {
                await eventPublisher.Publish(new DotAuthError(Id.Create(), cre.Code, cre.Message, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
                return Results.Redirect("/Authenticate/SendCode");
            }
            catch (Exception ex)
            {
                await eventPublisher.Publish(new DotAuthError(Id.Create(), "misconfigured_2fa", ex.Message, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
                throw new Exception(Strings.TwoFactorNotProperlyConfigured, ex);
            }
        }
        catch (Exception exception)
        {
            await eventPublisher.Publish(new DotAuthError(Id.Create(), InvalidCredentials, exception.Message, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
            modelState.AddModelError(InvalidCredentials, exception.Message);
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateIndexView, viewModel, modelState: modelState);
        }
    }

    internal static async Task<IResult> GetAuthenticateOpenId(
        HttpContext httpContext,
        string code,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IConsentRepository consentRepository,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILoggerFactory loggerFactory)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var dataProtector = dataProtectionProvider.CreateProtector("Request");
        if (string.IsNullOrWhiteSpace(code))
        {
            var vm = new AuthorizeOpenIdViewModel { Code = code };
            await SetIdProviders(authenticationSchemeProvider, vm).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateOpenIdView, vm);
        }

        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var dataString = Uri.UnescapeDataString(code);
        var request = dataProtector.Unprotect<AuthorizationRequest>(dataString);
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var action = new AuthenticateResourceOwnerOpenIdAction(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
        var actionResult = await action.Execute(request.ToParameter(), authenticatedUser!, code, issuerName, cancellationToken).ConfigureAwait(false);
        var result = actionResult.CreateRedirectionFromActionResult(request, logger);
        if (result != null)
        {
            await LogAuthenticateUser(eventPublisher, authenticatedUser!.GetSubject()!, actionResult.Amr).ConfigureAwait(false);
            return UiEndpointHelpers.ToRedirectResult(httpContext, result);
        }

        var viewModel = new AuthorizeOpenIdViewModel { Code = code };
        await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
        return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateOpenIdView, viewModel);
    }

    internal static async Task<IResult> PostLocalLoginOpenId(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IDataProtectionProvider dataProtectionProvider,
        IAuthenticationService authenticationService,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        IEventPublisher eventPublisher,
        IAuthorizationCodeStore authorizationCodeStore,
        IConsentRepository consentRepository,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var viewModel = await EndpointHandlerHelpers.BindFromFormAsync<OpenidLocalAuthenticationViewModel>(httpContext.Request).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(viewModel.Code))
        {
            throw new ArgumentException(Strings.MissingValues, nameof(viewModel));
        }

        await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var request = dataProtectionProvider.CreateProtector("Request").Unprotect<AuthorizationRequest>(viewModel.Code);
        var modelState = new ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(viewModel.Login))
        {
            modelState.AddModelError("Login", "the user name is required");
        }
        if (string.IsNullOrWhiteSpace(viewModel.Password))
        {
            modelState.AddModelError("Password", "the password is required");
        }

        if (!modelState.IsValid)
        {
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateOpenIdView, viewModel, modelState: modelState);
        }

        var services = resourceOwnerServices.ToArray();
        var localOpenIdAuthentication = new LocalOpenIdUserAuthenticationAction(
            authorizationCodeStore,
            services,
            consentRepository,
            tokenStore,
            scopeRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var actionResult = await localOpenIdAuthentication.Execute(
                new LocalAuthenticationParameter { UserName = viewModel.Login, Password = viewModel.Password },
                request.ToParameter(),
                viewModel.Code,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (actionResult.ErrorMessage == null)
        {
            var subject = actionResult.Claims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;
            if (!string.IsNullOrWhiteSpace(actionResult.TwoFactor))
            {
                try
                {
                    await UiEndpointHelpers.SetTwoFactorCookieAsync(httpContext, authenticationService, actionResult.Claims).ConfigureAwait(false);
                    var generateAndSendCode = new GenerateAndSendCodeAction(
                        httpContext.RequestServices.GetRequiredService<IResourceOwnerRepository>(),
                        httpContext.RequestServices.GetRequiredService<IConfirmationCodeStore>(),
                        httpContext.RequestServices.GetRequiredService<ITwoFactorAuthenticationHandler>(),
                        logger);
                    await generateAndSendCode.Send(subject, cancellationToken).ConfigureAwait(false);
                    return Results.Redirect($"/Authenticate/SendCode?code={Uri.EscapeDataString(viewModel.Code)}");
                }
                catch (ClaimRequiredException cre)
                {
                    await eventPublisher.Publish(new DotAuthError(Id.Create(), cre.Code, cre.Message, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
                    return Results.Redirect($"/Authenticate/SendCode?code={Uri.EscapeDataString(viewModel.Code)}");
                }
                catch (Exception ex)
                {
                    await eventPublisher.Publish(new DotAuthError(Id.Create(), ex.Message, ex.Message, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
                    modelState.AddModelError(InvalidCredentials, "Two factor authenticator is not properly configured");
                }
            }
            else
            {
                await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, actionResult.Claims, request.session_id!).ConfigureAwait(false);
                var endpointResult = actionResult.EndpointResult!;
                var result = endpointResult.CreateRedirectionFromActionResult(request, logger);
                if (result != null)
                {
                    await LogAuthenticateUser(eventPublisher, subject, endpointResult.Amr!).ConfigureAwait(false);
                    return UiEndpointHelpers.ToRedirectResult(httpContext, result);
                }
            }
        }
        else
        {
            await eventPublisher.Publish(new DotAuthError(Id.Create(), ErrorCodes.InvalidRequest, actionResult.ErrorMessage, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
            modelState.AddModelError(InvalidCredentials, actionResult.ErrorMessage);
        }

        await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
        return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateOpenIdView, viewModel, modelState: modelState);
    }

    internal static async Task<IResult> GetSendCode(
        HttpContext httpContext,
        string code,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILoggerFactory loggerFactory)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.TwoFactorCookieName).ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            logger.LogError("{Error}", Strings.TwoFactorAuthenticationCannotBePerformed);
            return UiEndpointHelpers.RedirectToError(Strings.TwoFactorAuthenticationCannotBePerformed, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        var getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        var resourceOwnerOption = await getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
        if (resourceOwnerOption is Option<ResourceOwner>.Error)
        {
            return Results.BadRequest();
        }

        var resourceOwner = ((Option<ResourceOwner>.Result)resourceOwnerOption).Item;
        var service = resourceOwner.TwoFactorAuthentication == null ? null : twoFactorAuthenticationHandler.Get(resourceOwner.TwoFactorAuthentication);
        if (service == null)
        {
            return Results.BadRequest();
        }

        var viewModel = new CodeViewModel { AuthRequestCode = code, ClaimName = service.RequiredClaim };
        var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == service.RequiredClaim);
        if (claim != null)
        {
            viewModel.ClaimValue = claim.Value;
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateSendCodeView, viewModel);
    }

    internal static async Task<IResult> PostSendCode(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IConsentRepository consentRepository,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        IDataProtectionProvider dataProtectionProvider,
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var codeViewModel = await EndpointHandlerHelpers.BindFromFormAsync<CodeViewModel>(httpContext.Request).ConfigureAwait(false);
        var modelState = new ModelStateDictionary();
        codeViewModel.Validate(modelState);
        if (!modelState.IsValid)
        {
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateSendCodeView, codeViewModel, modelState: modelState);
        }

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.TwoFactorCookieName).ConfigureAwait(false);
        if (authenticatedUser?.Identity?.IsAuthenticated != true)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.UnhandledExceptionCode,
                Detail = Strings.TwoFactorAuthenticationCannotBePerformed,
                Status = System.Net.HttpStatusCode.BadRequest
            });
        }

        var subject = authenticatedUser.GetSubject()!;
        var generateAndSendCode = new GenerateAndSendCodeAction(resourceOwnerRepository, confirmationCodeStore, twoFactorAuthenticationHandler, logger);
        var getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        var updateUserClaimsOperation = new UpdateUserClaimsOperation(resourceOwnerRepository, logger);
        if (codeViewModel.Action == CodeViewModel.ResendAction)
        {
            var option = await getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
            if (option is not Option<ResourceOwner>.Result ro)
            {
                return Results.BadRequest();
            }

            var resourceOwner = ro.Item;
            var claim = resourceOwner.Claims.FirstOrDefault(c => c.Type == codeViewModel.ClaimName);
            if (claim != null)
            {
                resourceOwner.Claims.Remove(claim);
            }

            resourceOwner.Claims = resourceOwner.Claims.Add(new Claim(codeViewModel.ClaimName!, codeViewModel.ClaimValue!));
            var claimsLst = resourceOwner.Claims.Select(c => new Claim(c.Type, c.Value));
            await updateUserClaimsOperation.Execute(subject, claimsLst, cancellationToken).ConfigureAwait(false);
            await generateAndSendCode.Send(subject, cancellationToken).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateSendCodeView, codeViewModel);
        }

        var validateConfirmationCode = new ValidateConfirmationCodeAction(confirmationCodeStore);
        if (!await validateConfirmationCode.Execute(codeViewModel.Code!, subject, cancellationToken).ConfigureAwait(false))
        {
            await eventPublisher.Publish(new TwoFactorAuthenticationFailed(Id.Create(), subject, codeViewModel.AuthRequestCode, codeViewModel.Code, DateTimeOffset.UtcNow)).ConfigureAwait(false);
            modelState.AddModelError("Code", "confirmation code is not valid");
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateSendCodeView, codeViewModel, modelState: modelState);
        }

        if (string.IsNullOrWhiteSpace(codeViewModel.Code) || !await confirmationCodeStore.Remove(codeViewModel.Code, subject, cancellationToken).ConfigureAwait(false))
        {
            modelState.AddModelError("Code", "an error occurred while trying to remove the code");
            return UiEndpointHelpers.ViewOrJson(httpContext, AuthenticateSendCodeView, codeViewModel, modelState: modelState);
        }

        await authenticationService.SignOutAsync(httpContext, CookieNames.TwoFactorCookieName, new AuthenticationProperties()).ConfigureAwait(false);
        var authenticatedUserClaims = authenticatedUser.Claims.ToArray();
        if (!string.IsNullOrWhiteSpace(codeViewModel.AuthRequestCode))
        {
            var request = dataProtectionProvider.CreateProtector("Request").Unprotect<AuthorizationRequest>(codeViewModel.AuthRequestCode);
            if (request.session_id == null)
            {
                return Results.BadRequest();
            }

            await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, authenticatedUserClaims, request.session_id).ConfigureAwait(false);
            var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
            var authenticateHelper = new AuthenticateHelper(authorizationCodeStore, tokenStore, scopeRepository, consentRepository, clientStore, jwksStore, eventPublisher, logger);
            var actionResult = await authenticateHelper.ProcessRedirection(request.ToParameter(), codeViewModel.AuthRequestCode, subject, authenticatedUserClaims, issuerName, cancellationToken).ConfigureAwait(false);
            await LogAuthenticateUser(eventPublisher, subject, actionResult.Amr).ConfigureAwait(false);
            var result = actionResult.CreateRedirectionFromActionResult(request, logger)!;
                    return UiEndpointHelpers.ToRedirectResult(httpContext, result);
        }

        await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, authenticatedUserClaims, Id.Create()).ConfigureAwait(false);
        return Results.Redirect("/User");
    }

    internal static async Task<IResult> Logout(HttpContext httpContext, IAuthenticationService authenticationService)
    {
        httpContext.Response.Cookies.Delete(CoreConstants.SessionId);
        await authenticationService.SignOutAsync(httpContext, CookieNames.CookieName, new AuthenticationProperties()).ConfigureAwait(false);
        return Results.Redirect("/");
    }

    internal static async Task<IResult> ExternalLogin(HttpContext httpContext, IAuthenticationService authenticationService)
    {
        var provider = await GetRequestValueAsync(httpContext.Request, "provider").ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = "The external authentication provider is required.",
                Status = System.Net.HttpStatusCode.BadRequest
            });
        }

        var redirectUrl = BuildAbsoluteUri(httpContext, "/authenticate/logincallback");
        await authenticationService.ChallengeAsync(httpContext, provider, new AuthenticationProperties { RedirectUri = redirectUrl }).ConfigureAwait(false);
        return Results.Empty;
    }

    internal static async Task<IResult> LoginCallback(
        HttpContext httpContext,
        string error,
        CancellationToken cancellationToken,
        IDataProtectionProvider dataProtectionProvider,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
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
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogError("{Error}", string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            return UiEndpointHelpers.RedirectToError(string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error), Strings.InternalServerError, ErrorCodes.InternalError);
        }

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext).ConfigureAwait(false);
        if (authenticatedUser == null)
        {
            return Results.Redirect("/Authenticate");
        }

        var externalSubject = authenticatedUser.GetSubject()!;
        var resourceOwner = await resourceOwnerRepository.Get(new ExternalAccountLink { Issuer = authenticatedUser.Identity!.AuthenticationType!, Subject = externalSubject }, cancellationToken).ConfigureAwait(false);
        var claims = authenticatedUser.Claims.ToList();
        if (resourceOwner != null)
        {
            claims = resourceOwner.Claims.ToList();
        }
        else
        {
            var addUser = new AddUserOperation(runtimeSettings, resourceOwnerRepository, accountFilters, subjectBuilder, eventPublisher);
            var externalClaims = authenticatedUser.Claims.Where(c => !string.IsNullOrWhiteSpace(c.Value)).ToArray();
            var userClaims = runtimeSettings.ClaimsIncludedInUserCreation
                .Except(externalClaims.Select(x => x.Type).ToOpenIdClaimType())
                .Select(x => new Claim(x, string.Empty))
                .Concat(externalClaims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)))
                .Concat(externalClaims.Any(x => x.Type == OpenIdClaimTypes.Email)
                    ? [new Claim("domain", externalClaims.First(x => x.Type == OpenIdClaimTypes.Email).Value[externalClaims.First(x => x.Type == OpenIdClaimTypes.Email).Value.LastIndexOf('@')..])]
                    : Array.Empty<Claim>())
                .ToOpenidClaims()
                .OrderBy(x => x.Type)
                .ToArray();
            var now = DateTimeOffset.UtcNow;
            var record = new ResourceOwner
            {
                Subject = Id.Create(),
                ExternalLogins = [new ExternalAccountLink { Subject = authenticatedUser.GetSubject()!, Issuer = authenticatedUser.Identity!.AuthenticationType!, ExternalClaims = authenticatedUser.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)).ToArray() }],
                Password = Id.Create().ToSha256Hash(string.Empty),
                IsLocalAccount = false,
                Claims = userClaims,
                CreateDateTime = now,
                UpdateDateTime = now,
                TwoFactorAuthentication = null
            };
            var (success, subject) = await addUser.Execute(record, cancellationToken).ConfigureAwait(false);
            if (!success || string.IsNullOrWhiteSpace(subject))
            {
                return Results.Redirect($"/Error?code=500&message={Uri.EscapeDataString(Strings.InternalServerError)}");
            }

            record.Password = string.Empty;
            await eventPublisher.Publish(new ExternalUserCreated(Id.Create(), record, now)).ConfigureAwait(false);
            var nameIdentifier = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                claims.Remove(nameIdentifier);
            }
            claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
            resourceOwner = await resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
        }

        await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
        if (resourceOwner != null && !string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
        {
            await UiEndpointHelpers.SetTwoFactorCookieAsync(httpContext, authenticationService, claims.ToArray()).ConfigureAwait(false);
            try
            {
                var generateAndSendCode = new GenerateAndSendCodeAction(resourceOwnerRepository, confirmationCodeStore, twoFactorAuthenticationHandler, logger);
                await generateAndSendCode.Send(resourceOwner.Subject, cancellationToken).ConfigureAwait(false);
                return Results.Redirect("/Authenticate/SendCode");
            }
            catch (ClaimRequiredException)
            {
                return Results.Redirect("/Authenticate/SendCode");
            }
        }

        await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, claims.ToOpenidClaims(), Id.Create()).ConfigureAwait(false);
        await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
        return httpContext.Request.Query.TryGetValue("ReturnUrl", out var returnUrl) ? Results.Redirect(returnUrl!) : Results.Redirect("/User");
    }

    internal static async Task<IResult> ExternalLoginOpenId(HttpContext httpContext, IAuthenticationService authenticationService, RuntimeSettings runtimeSettings, ILoggerFactory loggerFactory)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        var provider = await GetRequestValueAsync(httpContext.Request, "provider").ConfigureAwait(false);
        var code = await GetRequestValueAsync(httpContext.Request, "code").ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = "The external authentication provider is required.",
                Status = System.Net.HttpStatusCode.BadRequest
            });
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = "The OpenID authorization code is required.",
                Status = System.Net.HttpStatusCode.BadRequest
            });
        }

        var cookieValue = Id.Create();
        var cookieName = string.Format(ExternalAuthenticateCookieName, cookieValue);
        httpContext.Response.Cookies.Append(cookieName, code, new CookieOptions { Secure = !runtimeSettings.AllowHttp, HttpOnly = runtimeSettings.AllowHttp, Expires = DateTimeOffset.UtcNow.AddMinutes(5) });
        var redirectUrl = BuildAbsoluteUri(httpContext, $"/authenticate/logincallbackopenid?code={Uri.EscapeDataString(cookieValue)}");
        await authenticationService.ChallengeAsync(httpContext, provider, new AuthenticationProperties { RedirectUri = redirectUrl }).ConfigureAwait(false);
        logger.LogDebug("Redirecting to external provider: {Provider}, with redirect url: {RedirectUrl}", provider, redirectUrl);
        return Results.Empty;
    }

    internal static async Task<IResult> LoginCallbackOpenId(
        HttpContext httpContext,
        string? code,
        string? error,
        CancellationToken cancellationToken,
        IDataProtectionProvider dataProtectionProvider,
        IAuthenticationService authenticationService,
        IResourceOwnerRepository resourceOwnerRepository,
        IConfirmationCodeStore confirmationCodeStore,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        IAuthorizationCodeStore authorizationCodeStore,
        IConsentRepository consentRepository,
        IScopeRepository scopeRepository,
        ITokenStore tokenStore,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings,
        IEnumerable<IAccountFilter> accountFilters,
        ISubjectBuilder subjectBuilder)
    {
        var logger = CreateAuthenticateLogger(loggerFactory);
        ArgumentNullException.ThrowIfNull(code);
        var cookieName = string.Format(ExternalAuthenticateCookieName, code);
        var request = httpContext.Request.Cookies[cookieName];
        if (request == null)
        {
            logger.LogError("{Error}", Strings.TheRequestCannotBeExtractedFromTheCookie);
            return UiEndpointHelpers.RedirectToError(Strings.TheRequestCannotBeExtractedFromTheCookie, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        httpContext.Response.Cookies.Append(cookieName, string.Empty, new CookieOptions { HttpOnly = true, Secure = !runtimeSettings.AllowHttp, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddDays(-1) });
        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogError("{Error}", string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error));
            return UiEndpointHelpers.RedirectToError(string.Format(Strings.AnErrorHasBeenRaisedWhenTryingToAuthenticate, error), Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext).ConfigureAwait(false);
        if (authenticatedUser?.Identity?.IsAuthenticated != true || authenticatedUser.Identity is not ClaimsIdentity)
        {
            logger.LogError("{Msg}", Strings.TheUserNeedsToBeAuthenticated);
            return UiEndpointHelpers.RedirectToError(Strings.TheUserNeedsToBeAuthenticated, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        var claims = authenticatedUser.Claims.ToArray();
        var externalSubject = authenticatedUser.GetSubject();
        var resourceOwner = await resourceOwnerRepository.Get(new ExternalAccountLink { Issuer = authenticatedUser.Identity!.AuthenticationType!, Subject = externalSubject! }, cancellationToken).ConfigureAwait(false);
        var sub = string.Empty;
        if (resourceOwner == null)
        {
            var addUser = new AddUserOperation(runtimeSettings, resourceOwnerRepository, accountFilters, subjectBuilder, eventPublisher);
            var externalClaims = authenticatedUser.Claims.Where(c => !string.IsNullOrWhiteSpace(c.Value)).ToArray();
            var userClaims = runtimeSettings.ClaimsIncludedInUserCreation
                .Except(externalClaims.Select(x => x.Type).ToOpenIdClaimType())
                .Select(x => new Claim(x, string.Empty))
                .Concat(externalClaims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)))
                .ToOpenidClaims()
                .OrderBy(x => x.Type)
                .ToArray();
            var now = DateTimeOffset.UtcNow;
            var record = new ResourceOwner
            {
                Subject = Id.Create(),
                ExternalLogins = [new ExternalAccountLink { Subject = authenticatedUser.GetSubject()!, Issuer = authenticatedUser.Identity!.AuthenticationType!, ExternalClaims = authenticatedUser.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer)).ToArray() }],
                Password = Id.Create().ToSha256Hash(string.Empty),
                IsLocalAccount = false,
                Claims = userClaims,
                CreateDateTime = now,
                UpdateDateTime = now,
                TwoFactorAuthentication = null
            };
            var (success, s) = await addUser.Execute(record, cancellationToken).ConfigureAwait(false);
            if (!success || string.IsNullOrWhiteSpace(s))
            {
                return Results.Redirect("/Error");
            }
            sub = s;
            resourceOwner = await resourceOwnerRepository.Get(s, cancellationToken).ConfigureAwait(false);
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
            await UiEndpointHelpers.SetTwoFactorCookieAsync(httpContext, authenticationService, claims).ConfigureAwait(false);
            var generateAndSendCode = new GenerateAndSendCodeAction(resourceOwnerRepository, confirmationCodeStore, twoFactorAuthenticationHandler, logger);
            await generateAndSendCode.Send(resourceOwner.Subject, cancellationToken).ConfigureAwait(false);
            return Results.Redirect($"/Authenticate/SendCode?code={Uri.EscapeDataString(request)}");
        }

        var subject = resourceOwner!.Subject;
        var authorizationRequest = dataProtectionProvider.CreateProtector("Request").Unprotect<AuthorizationRequest>(request);
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var authenticateHelper = new AuthenticateHelper(authorizationCodeStore, tokenStore, scopeRepository, consentRepository, clientStore, jwksStore, eventPublisher, logger);
        var actionResult = await authenticateHelper.ProcessRedirection(authorizationRequest.ToParameter(), request, subject, claims, issuerName, cancellationToken).ConfigureAwait(false);
        await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, claims.ToOpenidClaims(), authorizationRequest.session_id!).ConfigureAwait(false);
        await authenticationService.SignOutAsync(httpContext, null, new AuthenticationProperties()).ConfigureAwait(false);
        await LogAuthenticateUser(eventPublisher, subject, actionResult.Amr!).ConfigureAwait(false);
        return UiEndpointHelpers.ToRedirectResult(
            httpContext,
            actionResult.CreateRedirectionFromActionResult(authorizationRequest, logger)!);
    }

    private static async Task SetIdProviders(IAuthenticationSchemeProvider authenticationSchemeProvider, IdProviderAuthorizeViewModel authorizeViewModel)
    {
        var schemes = (await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName) && !p.DisplayName.StartsWith('_'));
        authorizeViewModel.IdProviders = schemes.Select(scheme => new IdProviderViewModel { AuthenticationScheme = scheme.Name, DisplayName = scheme.DisplayName }).ToArray();
    }

    private static async Task LogAuthenticateUser(IEventPublisher eventPublisher, string resourceOwner, string? amr)
    {
        await eventPublisher.Publish(new ResourceOwnerAmrAuthenticated(Id.Create(), resourceOwner, amr, DateTimeOffset.UtcNow)).ConfigureAwait(false);
    }

    private static ILogger CreateAuthenticateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger("DotAuth.Controllers.AuthenticateController");
    }

    private static async Task<string?> GetRequestValueAsync(HttpRequest request, string key)
    {
        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync().ConfigureAwait(false);
            if (form.TryGetValue(key, out var formValue) && !string.IsNullOrWhiteSpace(formValue))
            {
                return formValue.ToString();
            }
        }

        return request.Query.TryGetValue(key, out var queryValue) && !string.IsNullOrWhiteSpace(queryValue)
            ? queryValue.ToString()
            : null;
    }

    private static string BuildAbsoluteUri(HttpContext httpContext, string pathAndQuery)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}{pathAndQuery}";
    }
}


