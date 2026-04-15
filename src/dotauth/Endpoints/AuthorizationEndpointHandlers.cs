namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Authorization;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Results;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Telemetry;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

internal static class AuthorizationEndpointHandlers
{
    internal static async Task<IResult> Get(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        [FromServices] IHttpClientFactory httpClientFactory,
        IEventPublisher eventPublisher,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        IClientStore clientStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IAuthorizationCodeStore authorizationCodeStore,
        IConsentRepository consentRepository,
        IJwksStore jwksStore,
        IDataProtectionProvider dataProtectionProvider,
        IAuthenticationService authenticationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        using var activity = DotAuthTelemetry.StartServerActivity("dotauth.authorization.request");
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var authorizationRequest = EndpointHandlerHelpers.BindFromQuery<AuthorizationRequest>(httpContext.Request);
        activity?.SetTag("dotauth.client_id", DotAuthTelemetry.Normalize(authorizationRequest.client_id));
        activity?.SetTag("dotauth.response_type", DotAuthTelemetry.Normalize(authorizationRequest.response_type));
        activity?.SetTag("dotauth.scope.requested", DotAuthTelemetry.Normalize(authorizationRequest.scope));

        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.AuthorizationController");
        var originUrl = GetOriginUrl(httpContext);
        var sessionId = GetSessionId(httpContext.Request);
        var result = await ResolveAuthorizationRequest(httpClientFactory, clientStore, jwksStore, authorizationRequest, cancellationToken).ConfigureAwait(false);
        if (result is Option<AuthorizationRequest>.Error e)
        {
            activity?.SetStatus(ActivityStatusCode.Error, e.Details.Title);
            return Results.Json(e.Details, statusCode: (int)e.Details.Status);
        }

        authorizationRequest = ((Option<AuthorizationRequest>.Result)result).Item with
        {
            origin_url = originUrl,
            session_id = sessionId
        };

        var authenticatedUser = await authenticationService.GetAuthenticatedUser(httpContext, CookieNames.CookieName).ConfigureAwait(false)
            ?? new ClaimsPrincipal();

        var parameter = authorizationRequest.ToParameter();
        var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
        var authorizationActions = new AuthorizationActions(
            authorizationCodeStore,
            clientStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            jwksStore,
            eventPublisher,
            resourceOwnerServices,
            logger);
        var actionResult = await authorizationActions.GetAuthorization(parameter, authenticatedUser, issuerName, cancellationToken).ConfigureAwait(false);

        switch (actionResult.Type)
        {
            case ActionResultType.RedirectToCallBackUrl:
                activity?.SetStatus(ActivityStatusCode.Ok);
                DotAuthTelemetry.RecordAuthorizationCodeIssued(authorizationRequest.client_id);
                return UiEndpointHelpers.ToRedirectResult(
                    httpContext,
                    authorizationRequest.redirect_uri!.CreateRedirectHttpTokenResponse(
                        actionResult.GetRedirectionParameters(),
                        actionResult.RedirectInstruction!.ResponseMode!));
            case ActionResultType.RedirectToAction:
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                DotAuthTelemetry.RecordAuthorizationCodeIssued(authorizationRequest.client_id);
                if (actionResult.RedirectInstruction!.Action == DotAuthEndPoints.AuthenticateIndex
                    || actionResult.RedirectInstruction.Action == DotAuthEndPoints.ConsentIndex)
                {
                    if (actionResult.RedirectInstruction.Action == DotAuthEndPoints.AuthenticateIndex)
                    {
                        authorizationRequest = authorizationRequest with { prompt = PromptParameters.Login };
                    }

                    if (!string.IsNullOrWhiteSpace(actionResult.ProcessId))
                    {
                        authorizationRequest = authorizationRequest with { aggregate_id = actionResult.ProcessId };
                    }

                    var encryptedRequest = dataProtectionProvider.CreateProtector("Request").Protect(authorizationRequest);
                    actionResult = actionResult with
                    {
                        RedirectInstruction = actionResult.RedirectInstruction.AddParameter(
                            StandardAuthorizationResponseNames.AuthorizationCodeName,
                            encryptedRequest)
                    };
                }

                var redirectionPath = UiEndpointHelpers.GetRedirectPathForEndpoint(actionResult.RedirectInstruction.Action, actionResult.Amr);
                var redirectionUrl = new Uri($"{httpContext.Request.GetAbsoluteUriWithVirtualPath()}{redirectionPath}")
                    .AddParametersInQuery(actionResult.GetRedirectionParameters());
                return Results.Redirect(redirectionUrl.AbsoluteUri);
            }
            case ActionResultType.BadRequest:
                activity?.SetStatus(ActivityStatusCode.Error, actionResult.Error?.Title);
                return Results.BadRequest(actionResult.Error);
            default:
                activity?.SetStatus(ActivityStatusCode.Error);
                return Results.BadRequest();
        }
    }

    private static string GetSessionId(HttpRequest request)
    {
        return request.Cookies.TryGetValue(CoreConstants.SessionId, out var sessionId)
            ? sessionId
            : Id.Create();
    }

    private static string? GetOriginUrl(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("Referer", out var referer))
        {
            return null;
        }

        var uri = new Uri(referer!);
        return $"{uri.Scheme}://{uri.Authority}";
    }

    private static async Task<AuthorizationRequest?> GetAuthorizationRequestFromJwt(
        IClientStore clientStore,
        IJwksStore jwksStore,
        string token,
        string clientId,
        CancellationToken cancellationToken)
    {
        var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return null;
        }

        var validationParameters = await client.CreateValidationParameters(jwksStore, cancellationToken: cancellationToken).ConfigureAwait(false);
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, validationParameters, out var securityToken);
        return (securityToken as JwtSecurityToken)?.Payload?.ToAuthorizationRequest();
    }

    private static async Task<Option<AuthorizationRequest>> ResolveAuthorizationRequest(
        IHttpClientFactory httpClientFactory,
        IClientStore clientStore,
        IJwksStore jwksStore,
        AuthorizationRequest authorizationRequest,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authorizationRequest.request))
        {
            var result = authorizationRequest.client_id == null
                ? null
                : await GetAuthorizationRequestFromJwt(clientStore, jwksStore, authorizationRequest.request, authorizationRequest.client_id, cancellationToken).ConfigureAwait(false);
            return result is null
                ? new Option<AuthorizationRequest>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Properties.Strings.TheRequestParameterIsNotCorrect,
                        Status = HttpStatusCode.BadRequest
                    },
                    authorizationRequest.state)
                : new Option<AuthorizationRequest>.Result(result);
        }

        if (authorizationRequest.request_uri == null)
        {
            return new Option<AuthorizationRequest>.Result(authorizationRequest);
        }

        if (authorizationRequest.request_uri.IsAbsoluteUri || authorizationRequest.request_uri.AbsoluteUri.StartsWith("//"))
        {
            return new Option<AuthorizationRequest>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequestUriCode,
                    Detail = Properties.Strings.TheRequestUriParameterIsNotWellFormed,
                    Status = HttpStatusCode.BadRequest
                },
                authorizationRequest.state);
        }

        var client = httpClientFactory.CreateClient();
        var httpResult = await client.GetAsync(authorizationRequest.request_uri, cancellationToken).ConfigureAwait(false);
        if (!httpResult.IsSuccessStatusCode)
        {
            return new Option<AuthorizationRequest>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = Properties.Strings.TheRequestDownloadedFromRequestUriIsNotValid,
                    Status = HttpStatusCode.BadRequest
                },
                authorizationRequest.state);
        }

        var token = await httpResult.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var result2 = authorizationRequest.client_id == null
            ? null
            : await GetAuthorizationRequestFromJwt(clientStore, jwksStore, token, authorizationRequest.client_id, cancellationToken).ConfigureAwait(false);
        return result2 == null
            ? new Option<AuthorizationRequest>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = Properties.Strings.TheRequestDownloadedFromRequestUriIsNotValid,
                    Status = HttpStatusCode.BadRequest
                },
                authorizationRequest.state)
            : new Option<AuthorizationRequest>.Result(result2);
    }
}






