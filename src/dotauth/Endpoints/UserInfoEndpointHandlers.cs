namespace DotAuth.Endpoints;

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Telemetry;
using Microsoft.AspNetCore.Http;

internal static class UserInfoEndpointHandlers
{
	internal static async Task<IResult> GetUserInfo(
		HttpContext httpContext,
		IRequestThrottle requestThrottle,
		ITokenStore tokenStore,
		CancellationToken cancellationToken)
	{
		using var activity = DotAuthTelemetry.StartServerActivity(DotAuthTelemetry.ActivityNames.UserInfoRequest);
		var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
		if (throttled != null)
		{
			return throttled;
		}

		var accessToken = await EndpointHandlerHelpers.TryGetAccessTokenAsync(httpContext.Request).ConfigureAwait(false);
		if (string.IsNullOrWhiteSpace(accessToken))
		{
			activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.InvalidToken);
			DotAuthTelemetry.RecordUserInfoRequest(false);
			return Results.BadRequest(new ErrorDetails { Title = ErrorCodes.InvalidToken, Detail = ErrorCodes.InvalidToken });
		}

		var grantedToken = await tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
		if (grantedToken == null)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ErrorCodes.InvalidToken);
			DotAuthTelemetry.RecordUserInfoRequest(false);
			return Results.BadRequest(new ErrorDetails { Detail = Strings.TheTokenIsNotValid, Title = ErrorCodes.InvalidToken });
		}

		activity?.SetStatus(ActivityStatusCode.Ok);
		DotAuthTelemetry.RecordUserInfoRequest(true);
		return Results.Json(grantedToken.UserInfoPayLoad ?? grantedToken.IdTokenPayLoad ?? new JwtPayload());
	}
}


