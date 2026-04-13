namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token;
using DotAuth.Common;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Policies;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

internal static class TokenEndpointHandlers
{
	internal static async Task<IResult> PostToken(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		RuntimeSettings settings,
		IAuthorizationCodeStore authorizationCodeStore,
		IClientStore clientStore,
		IScopeStore scopeStore,
		IResourceOwnerRepository resourceOwnerRepository,
		IEnumerable<IAuthenticateResourceOwnerService> authenticateResourceOwnerServices,
		ITokenStore tokenStore,
		ITicketStore ticketStore,
		IJwksStore jwksStore,
		IAuthorizationPolicyValidator authorizationPolicyValidator,
		IDeviceAuthorizationStore deviceAuthorizationStore,
		IEventPublisher eventPublisher,
		ILoggerFactory loggerFactory,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 0, ResponseCacheLocation.None, true);
		var tokenRequest = await EndpointHandlerHelpers.BindFromFormAsync<TokenRequest>(httpContext.Request).ConfigureAwait(false);
		if (tokenRequest.grant_type == null)
		{
			return Results.BadRequest(
				new ErrorDetails
				{
					Status = HttpStatusCode.BadRequest,
					Title = ErrorCodes.InvalidRequest,
					Detail = string.Format(Strings.MissingParameter, RequestTokenNames.GrantType)
				});
		}

		var logger = loggerFactory.CreateLogger("DotAuth.Api.Token");
		var tokenActions = new TokenActions(
			settings,
			authorizationCodeStore,
			clientStore,
			scopeStore,
			jwksStore,
			resourceOwnerRepository,
			authenticateResourceOwnerServices,
			eventPublisher,
			tokenStore,
			deviceAuthorizationStore,
			logger);
		var umaTokenActions = new UmaTokenActions(
			ticketStore,
			settings,
			clientStore,
			scopeStore,
			tokenStore,
			jwksStore,
			authorizationPolicyValidator,
			eventPublisher,
			logger);
		var certificate = httpContext.Request.GetCertificate();
		var authenticationHeaderValue = EndpointHandlerHelpers.TryGetAuthorizationHeader(httpContext.Request);
		var issuerName = httpContext.Request.GetAbsoluteUriWithVirtualPath();
		var result = await GetGrantedTokenAsync(
				tokenRequest,
				tokenActions,
				umaTokenActions,
				authenticationHeaderValue,
				certificate,
				issuerName,
				cancellationToken)
			.ConfigureAwait(false);
		if (result is Option<GrantedToken>.Result r)
		{
			return Results.Ok(r.Item.ToDto());
		}

		var error = (Option<GrantedToken>.Error)result;
		logger.LogError(
			"Could not issue token. {Title} - {Detail} - {Status}",
			error.Details.Title,
			error.Details.Detail,
			error.Details.Status);
		return Results.BadRequest(error.Details);
	}

	internal static async Task<IResult> RevokeToken(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		RuntimeSettings settings,
		IAuthorizationCodeStore authorizationCodeStore,
		IClientStore clientStore,
		IScopeStore scopeStore,
		IResourceOwnerRepository resourceOwnerRepository,
		IEnumerable<IAuthenticateResourceOwnerService> authenticateResourceOwnerServices,
		ITokenStore tokenStore,
		IJwksStore jwksStore,
		IDeviceAuthorizationStore deviceAuthorizationStore,
		IEventPublisher eventPublisher,
		ILoggerFactory loggerFactory,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 0, ResponseCacheLocation.None, true);
		var revocationRequest = await EndpointHandlerHelpers.BindFromFormAsync<RevocationRequest>(httpContext.Request)
			.ConfigureAwait(false);
		var logger = loggerFactory.CreateLogger("DotAuth.Api.Token");
		var tokenActions = new TokenActions(
			settings,
			authorizationCodeStore,
			clientStore,
			scopeStore,
			jwksStore,
			resourceOwnerRepository,
			authenticateResourceOwnerServices,
			eventPublisher,
			tokenStore,
			deviceAuthorizationStore,
			logger);
		var option = await tokenActions.RevokeToken(
				revocationRequest.ToParameter(),
				EndpointHandlerHelpers.TryGetAuthorizationHeader(httpContext.Request),
				httpContext.Request.GetCertificate(),
				httpContext.Request.GetAbsoluteUriWithVirtualPath(),
				cancellationToken)
			.ConfigureAwait(false);
		return option switch
		{
			Option.Success => Results.Ok(),
			Option.Error e => Results.BadRequest(e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private static async Task<Option<GrantedToken>> GetGrantedTokenAsync(
		TokenRequest tokenRequest,
		TokenActions tokenActions,
		UmaTokenActions umaTokenActions,
		AuthenticationHeaderValue? authenticationHeaderValue,
		X509Certificate2? certificate,
		string issuerName,
		CancellationToken cancellationToken)
	{
		switch (tokenRequest.grant_type)
		{
			case GrantTypes.Device:
				return await tokenActions.GetTokenByDeviceGrantType(
					tokenRequest.client_id,
					tokenRequest.device_code,
					issuerName,
					cancellationToken).ConfigureAwait(false);
			case GrantTypes.Password:
				return await tokenActions.GetTokenByResourceOwnerCredentialsGrantType(
						tokenRequest.ToResourceOwnerGrantTypeParameter(),
						authenticationHeaderValue,
						certificate,
						issuerName,
						cancellationToken)
					.ConfigureAwait(false);
			case GrantTypes.AuthorizationCode:
				return await tokenActions.GetTokenByAuthorizationCodeGrantType(
						tokenRequest.ToAuthorizationCodeGrantTypeParameter(),
						authenticationHeaderValue,
						certificate,
						issuerName,
						cancellationToken)
					.ConfigureAwait(false);
			case GrantTypes.RefreshToken:
				return await tokenActions.GetTokenByRefreshTokenGrantType(
						tokenRequest.ToRefreshTokenGrantTypeParameter(),
						authenticationHeaderValue,
						certificate,
						issuerName,
						cancellationToken)
					.ConfigureAwait(false);
			case GrantTypes.ClientCredentials:
				return await tokenActions.GetTokenByClientCredentialsGrantType(
						tokenRequest.ToClientCredentialsGrantTypeParameter(),
						authenticationHeaderValue,
						certificate,
						issuerName,
						cancellationToken)
					.ConfigureAwait(false);
			case GrantTypes.UmaTicket:
				return await umaTokenActions.GetTokenByTicketId(
						tokenRequest.ToTokenIdGrantTypeParameter(),
						authenticationHeaderValue,
						certificate,
						issuerName,
						cancellationToken)
					.ConfigureAwait(false);
			default:
				throw new ArgumentOutOfRangeException(nameof(tokenRequest));
		}
	}
}


