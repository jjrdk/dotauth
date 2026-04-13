namespace DotAuth.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Http;

internal static class UserInfoEndpointHandlers
{
	internal static async Task<IResult> GetUserInfo(
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

		var accessToken = await EndpointHandlerHelpers.TryGetAccessTokenAsync(httpContext.Request).ConfigureAwait(false);
		if (string.IsNullOrWhiteSpace(accessToken))
		{
			return Results.BadRequest(new ErrorDetails { Title = ErrorCodes.InvalidToken, Detail = ErrorCodes.InvalidToken });
		}

		var grantedToken = await tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
		if (grantedToken == null)
		{
			return Results.BadRequest(new ErrorDetails { Detail = Strings.TheTokenIsNotValid, Title = ErrorCodes.InvalidToken });
		}

		return Results.Json(grantedToken.UserInfoPayLoad ?? grantedToken.IdTokenPayLoad ?? new JwtPayload());
	}
}


