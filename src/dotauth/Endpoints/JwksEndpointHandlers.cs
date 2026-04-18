namespace DotAuth.Endpoints;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

internal static class JwksEndpointHandlers
{
	internal static async Task<IResult> GetJwks(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		IJwksRepository jwksStore,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 300, ResponseCacheLocation.Client, false);
		var jwks = await jwksStore.GetPublicKeys(cancellationToken).ConfigureAwait(false);
		return Results.Json(jwks);
	}

	internal static async Task<IResult> AddJwk(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		IJwksRepository jwksStore,
		JsonWebKey jsonWebKey,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var result = await jwksStore.Add(jsonWebKey, cancellationToken).ConfigureAwait(false);
		return result ? Results.Ok() : Results.BadRequest();
	}

	internal static async Task<IResult> RotateJwks(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		IJwksRepository jwksStore,
		JsonWebKeySet jsonWebKeySet,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var result = await jwksStore.Rotate(jsonWebKeySet, cancellationToken).ConfigureAwait(false);
		return result ? Results.Ok() : Results.BadRequest();
	}
}

