namespace DotAuth.Endpoints;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Introspection;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.Telemetry;
using Microsoft.AspNetCore.Http;

internal static class IntrospectionEndpointHandlers
{
	internal static async Task<IResult> PostIntrospection(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		ITokenStore tokenStore,
		CancellationToken cancellationToken)
	{
		using var activity = DotAuthTelemetry.StartServerActivity("dotauth.introspection.request");
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var introspectionRequest = await EndpointHandlerHelpers.BindFromFormAsync<IntrospectionRequest>(httpContext.Request)
			.ConfigureAwait(false);
		activity?.SetTag("dotauth.token_type_hint", DotAuthTelemetry.Normalize(introspectionRequest.token_type_hint));
		if (introspectionRequest.token == null)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.InvalidRequest);
			return EndpointHandlerHelpers.BuildJsonError(
				ErrorCodes.InvalidRequest,
				"no parameter in body request",
				HttpStatusCode.BadRequest);
		}

		var introspectionAction = new PostIntrospectionAction(tokenStore);
		var result = await introspectionAction.Execute(introspectionRequest.ToParameter(), cancellationToken).ConfigureAwait(false);
		return result switch
		{
			Option<OauthIntrospectionResponse>.Result r => CompleteOauthIntrospection(activity, r.Item),
			Option<OauthIntrospectionResponse>.Error e => CompleteIntrospectionError(activity, e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	internal static async Task<IResult> PostUmaIntrospection(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		ITokenStore tokenStore,
		CancellationToken cancellationToken)
	{
		using var activity = DotAuthTelemetry.StartServerActivity("dotauth.introspection.request");
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var introspectionRequest = await EndpointHandlerHelpers.BindFromFormAsync<IntrospectionRequest>(httpContext.Request)
			.ConfigureAwait(false);
		activity?.SetTag("dotauth.token_type_hint", DotAuthTelemetry.Normalize(introspectionRequest.token_type_hint));
		if (introspectionRequest.token == null)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.InvalidRequest);
			return EndpointHandlerHelpers.BuildJsonError(
				ErrorCodes.InvalidRequest,
				Strings.NoParameterInBodyRequest,
				HttpStatusCode.BadRequest);
		}

		var introspectionAction = new UmaIntrospectionAction(tokenStore);
		var result = await introspectionAction.Execute(introspectionRequest.ToParameter(), cancellationToken).ConfigureAwait(false);
		return result switch
		{
			Option<UmaIntrospectionResponse>.Result r => CompleteUmaIntrospection(activity, r.Item),
			Option<UmaIntrospectionResponse>.Error e => CompleteIntrospectionError(activity, e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private static IResult CompleteOauthIntrospection(Activity? activity, OauthIntrospectionResponse response)
	{
		activity?.SetStatus(ActivityStatusCode.Ok);
		DotAuthTelemetry.RecordIntrospectionRequest(response.Active, response.Active);
		return Results.Ok(response);
	}

	private static IResult CompleteUmaIntrospection(Activity? activity, UmaIntrospectionResponse response)
	{
		activity?.SetStatus(ActivityStatusCode.Ok);
		DotAuthTelemetry.RecordIntrospectionRequest(response.Active, response.Active);
		return Results.Ok(response);
	}

	private static IResult CompleteIntrospectionError(Activity? activity, DotAuth.Shared.Models.ErrorDetails errorDetails)
	{
		activity?.SetStatus(ActivityStatusCode.Error, errorDetails.Title);
		DotAuthTelemetry.RecordIntrospectionRequest(false, false);
		return Results.BadRequest(errorDetails);
	}
}


