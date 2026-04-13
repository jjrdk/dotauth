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
using DotAuth.Sms;
using DotAuth.Sms.Actions;
using DotAuth.Sms.Properties;
using DotAuth.Sms.ViewModels;
using DotAuth.ViewModels;
using DotAuth.WebSite.Authenticate;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class SmsUiEndpointHandlers
{
    private const string SmsAuthenticateIndexView = "/Areas/sms/Views/Authenticate/Index.cshtml";
    private const string SmsAuthenticateOpenIdView = "/Areas/sms/Views/Authenticate/OpenId.cshtml";
    private const string SmsConfirmCodeView = "/Areas/sms/Views/Authenticate/ConfirmCode.cshtml";

    internal static async Task<IResult> GetAuthenticateIndex(HttpContext httpContext, IAuthenticationService authenticationService, IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsAuthenticateIndexView, viewModel);
        }

        return Results.Redirect("/pwd/User");
    }

    internal static async Task<IResult> PostLocalLogin(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        RuntimeSettings runtimeSettings,
        IEventPublisher eventPublisher,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ISubjectBuilder subjectBuilder,
        IEnumerable<IAccountFilter> accountFilters,
        ISmsClient smsClient,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Sms.Controllers.AuthenticateController");
        var localAuthenticationViewModel = await EndpointHandlerHelpers.BindFromFormAsync<SmsAuthenticationViewModel>(httpContext.Request).ConfigureAwait(false);
        var authenticatedUser = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (authenticatedUser?.Identity is { IsAuthenticated: true })
        {
            return Results.Redirect("/pwd/User");
        }

        var modelState = new ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(localAuthenticationViewModel.PhoneNumber))
        {
            modelState.AddModelError("PhoneNumber", SmsStrings.MissingPhoneNumber);
        }

        if (!modelState.IsValid)
        {
            var viewModel = new AuthorizeViewModel();
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsAuthenticateIndexView, viewModel, modelState: modelState);
        }

        var smsAuthenticationOperation = new SmsAuthenticationOperation(runtimeSettings, smsClient, confirmationCodeStore, resourceOwnerRepository, subjectBuilder, accountFilters.ToArray(), eventPublisher, logger);
        ResourceOwner? resourceOwner = null;
        var option = await smsAuthenticationOperation.Execute(localAuthenticationViewModel.PhoneNumber!, cancellationToken).ConfigureAwait(false);
        if (option is Option<ResourceOwner>.Error e)
        {
            await eventPublisher.Publish(new DotAuthError(Id.Create(), e.Details.Title, e.Details.Detail, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
            modelState.AddModelError("message_error", e.Details.Detail);
        }
        else
        {
            resourceOwner = ((Option<ResourceOwner>.Result)option).Item;
        }

        if (resourceOwner != null)
        {
            resourceOwner.Claims = resourceOwner.Claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer));
            await SetPasswordLessCookie(httpContext, authenticationService, resourceOwner.Claims).ConfigureAwait(false);
            return Results.Redirect("/sms/Authenticate/ConfirmCode");
        }

        var authViewModel = new AuthorizeViewModel();
        await SetIdProviders(authenticationSchemeProvider, authViewModel).ConfigureAwait(false);
        return UiEndpointHelpers.ViewOrJson(httpContext, SmsAuthenticateIndexView, authViewModel, modelState: modelState);
    }

    internal static async Task<IResult> GetConfirmCode(HttpContext httpContext, string code, IAuthenticationService authenticationService, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Sms.Controllers.AuthenticateController");
        var user = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (user?.Identity is { IsAuthenticated: true })
        {
            return Results.Redirect("/pwd/User");
        }

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.PasswordLessCookieName).ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            const string message = "SMS authentication cannot be performed";
            logger.LogError("{Msg}", message);
            return UiEndpointHelpers.RedirectToError(message, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, SmsConfirmCodeView, new ConfirmCodeViewModel { Code = code });
    }

    internal static async Task<IResult> PostConfirmCode(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IConsentRepository consentRepository,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ITwoFactorAuthenticationHandler twoFactorAuthenticationHandler,
        ILoggerFactory loggerFactory,
        RuntimeSettings runtimeSettings)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Sms.Controllers.AuthenticateController");
        var confirmCodeViewModel = await EndpointHandlerHelpers.BindFromFormAsync<ConfirmCodeViewModel>(httpContext.Request).ConfigureAwait(false);
        var user = await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        if (user?.Identity is { IsAuthenticated: true })
        {
            return Results.Redirect("/pwd/User");
        }

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.PasswordLessCookieName).ConfigureAwait(false);
        if (authenticatedUser?.Identity is not { IsAuthenticated: true })
        {
            const string message = "SMS authentication cannot be performed";
            logger.LogError("{Msg}", message);
            return UiEndpointHelpers.RedirectToError(message, Strings.InternalServerError, ErrorCodes.UnhandledExceptionCode);
        }

        var modelState = new ModelStateDictionary();
        var authenticatedUserClaims = authenticatedUser.Claims.ToArray();
        var subject = authenticatedUserClaims.First(c => c.Type == OpenIdClaimTypes.Subject).Value;
        var phoneNumber = authenticatedUserClaims.First(c => c.Type == OpenIdClaimTypes.PhoneNumber);
        if (confirmCodeViewModel.Action == "resend")
        {
            var generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(httpContext.RequestServices.GetRequiredService<ISmsClient>(), confirmationCodeStore, logger);
            _ = await generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, cancellationToken).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsConfirmCodeView, confirmCodeViewModel);
        }

        var validateConfirmationCode = new ValidateConfirmationCodeAction(confirmationCodeStore);
        if (confirmCodeViewModel.ConfirmationCode == null || !await validateConfirmationCode.Execute(confirmCodeViewModel.ConfirmationCode, subject, cancellationToken).ConfigureAwait(false))
        {
            modelState.AddModelError("message_error", "Confirmation code is not valid");
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsConfirmCodeView, confirmCodeViewModel, modelState: modelState);
        }

        await authenticationService.SignOutAsync(httpContext, CookieNames.PasswordLessCookieName, new AuthenticationProperties()).ConfigureAwait(false);
        var getUserOperation = new GetUserOperation(resourceOwnerRepository, logger);
        var resourceOwnerOption = await getUserOperation.Execute(authenticatedUser, cancellationToken).ConfigureAwait(false);
        if (resourceOwnerOption is Option<ResourceOwner>.Error e)
        {
            return UiEndpointHelpers.RedirectToError(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
        }

        var resourceOwner = (resourceOwnerOption as Option<ResourceOwner>.Result)!.Item;
        if (!string.IsNullOrWhiteSpace(resourceOwner.TwoFactorAuthentication))
        {
            try
            {
                await UiEndpointHelpers.SetTwoFactorCookieAsync(httpContext, authenticationService, authenticatedUserClaims).ConfigureAwait(false);
                var generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(httpContext.RequestServices.GetRequiredService<ISmsClient>(), confirmationCodeStore, logger);
                await generateAndSendSmsCodeOperation.Execute(phoneNumber.Value, cancellationToken).ConfigureAwait(false);
                return Results.Redirect($"/Authenticate/SendCode?code={Uri.EscapeDataString(confirmCodeViewModel.Code ?? string.Empty)}");
            }
            catch (ClaimRequiredException)
            {
                return Results.Redirect($"/Authenticate/SendCode?code={Uri.EscapeDataString(confirmCodeViewModel.Code ?? string.Empty)}");
            }
            catch (Exception)
            {
                modelState.AddModelError("message_error", "Two factor authenticator is not properly configured");
                return UiEndpointHelpers.ViewOrJson(httpContext, SmsConfirmCodeView, confirmCodeViewModel, modelState: modelState);
            }
        }

        if (!string.IsNullOrWhiteSpace(confirmCodeViewModel.Code))
        {
            var request = dataProtectionProvider.CreateProtector("Request").Unprotect<AuthorizationRequest>(confirmCodeViewModel.Code);
            await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, authenticatedUserClaims, request.session_id!).ConfigureAwait(false);
            var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
            var authenticateHelper = new AuthenticateHelper(authorizationCodeStore, tokenStore, scopeRepository, consentRepository, clientStore, jwksStore, eventPublisher, logger);
            var actionResult = await authenticateHelper.ProcessRedirection(request.ToParameter(), confirmCodeViewModel.Code, subject, authenticatedUserClaims, issuerName, cancellationToken).ConfigureAwait(false);
            var result = actionResult.CreateRedirectionFromActionResult(request, logger);
            if (result != null)
            {
                await eventPublisher.Publish(new ResourceOwnerAmrAuthenticated(Id.Create(), resourceOwner.Subject, actionResult.Amr!, DateTimeOffset.UtcNow)).ConfigureAwait(false);
                return UiEndpointHelpers.ToRedirectResult(httpContext, result);
            }
        }

        await UiEndpointHelpers.SetLocalCookieAsync(httpContext, authenticationService, runtimeSettings, authenticatedUserClaims, Id.Create()).ConfigureAwait(false);
        var modelCode = string.IsNullOrWhiteSpace(confirmCodeViewModel.Code) ? confirmCodeViewModel.ConfirmationCode : confirmCodeViewModel.Code;
        if (!string.IsNullOrWhiteSpace(modelCode))
        {
            await confirmationCodeStore.Remove(modelCode, subject, cancellationToken).ConfigureAwait(false);
        }

        return Results.Redirect("/pwd/User");
    }

    internal static async Task<IResult> PostLocalLoginOpenId(
        HttpContext httpContext,
        CancellationToken cancellationToken,
        IAuthenticationService authenticationService,
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        RuntimeSettings runtimeSettings,
        IEventPublisher eventPublisher,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ISubjectBuilder subjectBuilder,
        IEnumerable<IAccountFilter> accountFilters,
        ISmsClient smsClient,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Sms.Controllers.AuthenticateController");
        var viewModel = await EndpointHandlerHelpers.BindFromFormAsync<SmsOpenIdLocalAuthenticationViewModel>(httpContext.Request).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(viewModel.Code))
        {
            throw new ArgumentException(ErrorMessages.InvalidCode, nameof(viewModel));
        }

        await UiEndpointHelpers.SetUserAsync(httpContext, authenticationService).ConfigureAwait(false);
        var modelState = new ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(viewModel.PhoneNumber))
        {
            modelState.AddModelError("message_error", SmsStrings.MissingPhoneNumber);
        }

        if (!modelState.IsValid)
        {
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsAuthenticateOpenIdView, viewModel, modelState: modelState);
        }

        var smsAuthenticationOperation = new SmsAuthenticationOperation(runtimeSettings, smsClient, confirmationCodeStore, resourceOwnerRepository, subjectBuilder, accountFilters.ToArray(), eventPublisher, logger);
        var option = await smsAuthenticationOperation.Execute(viewModel.PhoneNumber!, cancellationToken).ConfigureAwait(false);
        if (option is Option<ResourceOwner>.Error ex)
        {
            await eventPublisher.Publish(new DotAuthError(Id.Create(), ex.Details.Title, ex.Details.Detail, string.Empty, DateTimeOffset.UtcNow)).ConfigureAwait(false);
            modelState.AddModelError("message_error", ex.Details.Detail);
            await SetIdProviders(authenticationSchemeProvider, viewModel).ConfigureAwait(false);
            return UiEndpointHelpers.ViewOrJson(httpContext, SmsAuthenticateOpenIdView, viewModel, modelState: modelState);
        }

        var resourceOwner = ((Option<ResourceOwner>.Result)option).Item;
        resourceOwner.Claims = resourceOwner.Claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer));
        await SetPasswordLessCookie(httpContext, authenticationService, resourceOwner.Claims).ConfigureAwait(false);
        return Results.Redirect($"/sms/Authenticate/ConfirmCode?code={Uri.EscapeDataString(viewModel.Code ?? string.Empty)}");
    }

    internal static async Task<IResult> SendConfirmationCode(
        [FromBody] ConfirmationCodeRequest confirmationCodeRequest,
        CancellationToken cancellationToken,
        RuntimeSettings settings,
        ISmsClient smsClient,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ISubjectBuilder subjectBuilder,
        IEnumerable<IAccountFilter> accountFilters,
        IEventPublisher eventPublisher,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("DotAuth.Sms.Controllers.CodeController");
        if (string.IsNullOrWhiteSpace(confirmationCodeRequest.PhoneNumber))
        {
            return Results.Json(new ErrorDetails { Title = ErrorCodes.InvalidRequest, Detail = "parameter phone_number is missing", Status = System.Net.HttpStatusCode.BadRequest }, statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var operation = new SmsAuthenticationOperation(settings, smsClient, confirmationCodeStore, resourceOwnerRepository, subjectBuilder, accountFilters.ToArray(), eventPublisher, logger);
            var option = await operation.Execute(confirmationCodeRequest.PhoneNumber, cancellationToken).ConfigureAwait(false);
            if (option is Option<ResourceOwner>.Error e)
            {
                return Results.Json(e.Details, statusCode: (int)e.Details.Status);
            }

            return Results.Ok();
        }
        catch (Exception)
        {
            return Results.Json(new ErrorDetails { Title = ErrorCodes.UnhandledExceptionCode, Detail = "unhandled exception occurred please contact the administrator", Status = System.Net.HttpStatusCode.InternalServerError }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task SetIdProviders(IAuthenticationSchemeProvider authenticationSchemeProvider, IdProviderAuthorizeViewModel authorizeViewModel)
    {
        var schemes = (await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false)).Where(p => !string.IsNullOrWhiteSpace(p.DisplayName) && !p.DisplayName.StartsWith('_'));
        authorizeViewModel.IdProviders = schemes.Select(scheme => new IdProviderViewModel { AuthenticationScheme = scheme.Name, DisplayName = scheme.DisplayName }).ToArray();
    }

    private static async Task SetPasswordLessCookie(HttpContext httpContext, IAuthenticationService authenticationService, IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, CookieNames.PasswordLessCookieName);
        var principal = new ClaimsPrincipal(identity);
        await authenticationService.SignInAsync(httpContext, CookieNames.PasswordLessCookieName, principal, new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20), IsPersistent = false }).ConfigureAwait(false);
    }
}



