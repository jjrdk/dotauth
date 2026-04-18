namespace DotAuth.Endpoints;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Device;
using DotAuth.Common;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

internal static class DeviceAuthorizationEndpointHandlers
{
	internal static async Task<IResult> RequestDeviceAuthorization(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		RuntimeSettings settings,
		IClientStore clientStore,
		IDeviceAuthorizationStore deviceAuthorizationStore,
		ILoggerFactory loggerFactory,
		CancellationToken cancellationToken)
	{
		using var activity = DotAuthTelemetry.StartServerActivity(DotAuthTelemetry.ActivityNames.DeviceAuthorizationRequest);
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 0, ResponseCacheLocation.None, true);
		var request = await EndpointHandlerHelpers.BindFromFormAsync<TokenRequest>(httpContext.Request).ConfigureAwait(false);
		activity?.SetTag(DotAuthTelemetry.TagKeys.ClientId, DotAuthTelemetry.Normalize(request.client_id));
		activity?.SetTag(DotAuthTelemetry.TagKeys.ScopeRequested, DotAuthTelemetry.Normalize(request.scope));
		if (string.IsNullOrWhiteSpace(request.client_id))
		{
			activity?.SetTag(DotAuthTelemetry.TagKeys.ErrorCode, ErrorCodes.InvalidRequest);
			activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.InvalidRequest);
			return Results.BadRequest();
		}

		var logger = loggerFactory.CreateLogger("DotAuth.Api.DeviceAuthorization");
		var actions = new DeviceAuthorizationActions(settings, deviceAuthorizationStore, clientStore, logger);
		var scopeArray = string.IsNullOrWhiteSpace(request.scope)
			? []
			: request.scope.Split(' ', StringSplitOptions.TrimEntries).ToArray();
		var response = await actions.StartDeviceAuthorizationRequest(
				request.client_id,
				httpContext.Request.GetAbsoluteUri(),
				scopeArray,
				cancellationToken)
			.ConfigureAwait(false);
		return response switch
		{
			Option<DeviceAuthorizationData>.Result r => CompleteDeviceAuthorization(activity, request.client_id, r.Item),
			Option<DeviceAuthorizationData>.Error e => CompleteDeviceAuthorizationError(activity, e.Details),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private static IResult CompleteDeviceAuthorization(Activity? activity, string clientId, DeviceAuthorizationData authorizationData)
	{
		activity?.SetStatus(ActivityStatusCode.Ok);
		DotAuthTelemetry.RecordDeviceAuthorizationStarted(clientId);
		return Results.Ok(authorizationData.Response);
	}

	private static IResult CompleteDeviceAuthorizationError(
		Activity? activity,
		DotAuth.Shared.Models.ErrorDetails errorDetails)
	{
		activity?.SetTag(DotAuthTelemetry.TagKeys.ErrorCode, DotAuthTelemetry.Normalize(errorDetails.Title));
		activity?.SetStatus(ActivityStatusCode.Error, errorDetails.Detail);
		return Results.Json(errorDetails, statusCode: (int)errorDetails.Status);
	}
}



