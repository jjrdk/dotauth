namespace DotAuth.Endpoints;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Discovery;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

internal static class DiscoveryEndpointHandlers
{
	internal static async Task<IResult> GetDiscovery(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		IScopeRepository scopeRepository,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 86400, ResponseCacheLocation.Any, false);
		var discoveryActions = new DiscoveryActions(scopeRepository);
		var issuer = httpContext.Request.GetAbsoluteUriWithVirtualPath();
		var result = await discoveryActions.CreateDiscoveryInformation(issuer, cancellationToken).ConfigureAwait(false);
		return Results.Json(result);
	}

	internal static async Task<IResult> GetUmaConfiguration(
		HttpContext httpContext,
		IScopeStore scopeStore,
		CancellationToken cancellationToken)
	{
		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 86400, ResponseCacheLocation.Any, false);
		var absoluteUriWithVirtualPath = httpContext.Request.GetAbsoluteUriWithVirtualPath();
		var scopes = await scopeStore.GetAll(cancellationToken).ConfigureAwait(false);
		var scopeSupportedNames = scopes != null && scopes.Any()
			? scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray()
			: [];
		var result = new UmaConfiguration
		{
			ClaimTokenProfilesSupported = [],
			UmaProfilesSupported =
			[
				"https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
			],
			ResourceRegistrationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.ResourceSet}"),
			PermissionEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.Permission}"),
			ScopesSupported = scopeSupportedNames,
			Issuer = new Uri(absoluteUriWithVirtualPath),
			AuthorizationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Authorization}"),
			TokenEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Token}"),
			JwksUri = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Jwks}"),
			RegistrationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Clients}"),
			IntrospectionEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.Introspection}"),
			RevocationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Token}/revoke"),
			UiLocalesSupported = ["en"],
			GrantTypesSupported = GrantTypes.All,
			ResponseTypesSupported = ResponseTypeNames.All
		};
		return Results.Ok(result);
	}
}


