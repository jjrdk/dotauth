namespace DotAuth.Endpoints;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Device;
using DotAuth.Common;
using DotAuth.Extensions;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
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
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		EndpointHandlerHelpers.SetCacheHeaders(httpContext.Response, 0, ResponseCacheLocation.None, true);
		var request = await EndpointHandlerHelpers.BindFromFormAsync<TokenRequest>(httpContext.Request).ConfigureAwait(false);
		if (string.IsNullOrWhiteSpace(request.client_id))
		{
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
			Option<DeviceAuthorizationData>.Result r => Results.Ok(r.Item.Response),
			Option<DeviceAuthorizationData>.Error e => Results.Json(e.Details, statusCode: (int)e.Details.Status),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}



