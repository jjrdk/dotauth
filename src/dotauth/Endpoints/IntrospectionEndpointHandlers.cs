namespace DotAuth.Endpoints;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Introspection;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Http;

internal static class IntrospectionEndpointHandlers
{
	internal static async Task<IResult> PostIntrospection(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		ITokenStore tokenStore,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var introspectionRequest = await EndpointHandlerHelpers.BindFromFormAsync<IntrospectionRequest>(httpContext.Request)
			.ConfigureAwait(false);
		if (introspectionRequest.token == null)
		{
			return EndpointHandlerHelpers.BuildJsonError(
				ErrorCodes.InvalidRequest,
				"no parameter in body request",
				HttpStatusCode.BadRequest);
		}

		var introspectionAction = new PostIntrospectionAction(tokenStore);
		var result = await introspectionAction.Execute(introspectionRequest.ToParameter(), cancellationToken).ConfigureAwait(false);
		return result switch
		{
			Option<OauthIntrospectionResponse>.Result r => Results.Ok(r.Item),
			Option<OauthIntrospectionResponse>.Error e => Results.BadRequest(e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	internal static async Task<IResult> PostUmaIntrospection(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		ITokenStore tokenStore,
		CancellationToken cancellationToken)
	{
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var introspectionRequest = await EndpointHandlerHelpers.BindFromFormAsync<IntrospectionRequest>(httpContext.Request)
			.ConfigureAwait(false);
		if (introspectionRequest.token == null)
		{
			return EndpointHandlerHelpers.BuildJsonError(
				ErrorCodes.InvalidRequest,
				Strings.NoParameterInBodyRequest,
				HttpStatusCode.BadRequest);
		}

		var introspectionAction = new UmaIntrospectionAction(tokenStore);
		var result = await introspectionAction.Execute(introspectionRequest.ToParameter(), cancellationToken).ConfigureAwait(false);
		return result switch
		{
			Option<UmaIntrospectionResponse>.Result r => Results.Ok(r.Item),
			Option<UmaIntrospectionResponse>.Error e => Results.BadRequest(e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}


