namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

internal static class EndpointHandlerHelpers
{
	internal static async Task<IResult?> TryThrottleAsync(HttpContext httpContext, IRequestThrottle requestThrottle)
	{
		return await requestThrottle.Allow(httpContext.Request).ConfigureAwait(false)
			? null
			: Results.StatusCode(StatusCodes.Status429TooManyRequests);
	}

	internal static IResult BuildJsonError(string code, string message, HttpStatusCode statusCode)
	{
		var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
		return Results.Json(error, statusCode: (int)statusCode);
	}

	internal static async Task<T> BindFromFormAsync<T>(HttpRequest request)
		where T : new()
	{
		if (!request.HasFormContentType)
		{
			return new T();
		}

		var form = await request.ReadFormAsync().ConfigureAwait(false);
		var result = new T();
		foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			var bindingName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
			if (!form.TryGetValue(bindingName, out var values))
			{
				continue;
			}

			var value = values.LastOrDefault();
			var converted = ConvertValue(property.PropertyType, value);
			if (converted != null)
			{
				property.SetValue(result, converted);
			}
		}

		return result;
	}

	internal static T BindFromQuery<T>(HttpRequest request)
		where T : new()
	{
		var result = new T();
		foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			var bindingName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
			if (!request.Query.TryGetValue(bindingName, out var values))
			{
				continue;
			}

			var value = values.LastOrDefault();
			var converted = ConvertValue(property.PropertyType, value);
			if (converted != null)
			{
				property.SetValue(result, converted);
			}
		}

		return result;
	}

	internal static AuthenticationHeaderValue? TryGetAuthorizationHeader(HttpRequest request)
	{
		if (!request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
		{
			return null;
		}

		return AuthenticationHeaderValue.TryParse(authorizationHeader[0], out var header)
			? header
			: null;
	}

	internal static async Task<string> TryGetAccessTokenAsync(HttpRequest request)
	{
		var accessToken = GetAccessTokenFromAuthorizationHeader(request);
		if (!string.IsNullOrWhiteSpace(accessToken))
		{
			return accessToken;
		}

		accessToken = await GetAccessTokenFromBodyParameterAsync(request).ConfigureAwait(false);
		return string.IsNullOrWhiteSpace(accessToken) ? GetAccessTokenFromQueryString(request) : accessToken;
	}

	internal static void SetCacheHeaders(HttpResponse response, int duration, ResponseCacheLocation location, bool noStore)
	{
		response.Headers.Remove(HeaderNames.CacheControl);
		response.Headers.Remove(HeaderNames.Pragma);

		if (noStore)
		{
			response.Headers[HeaderNames.CacheControl] = "no-store,no-cache";
			response.Headers[HeaderNames.Pragma] = "no-cache";
			return;
		}

		var visibility = location switch
		{
			ResponseCacheLocation.Any => "public",
			ResponseCacheLocation.Client => "private",
			ResponseCacheLocation.None => "no-cache",
			_ => "public"
		};
		response.Headers[HeaderNames.CacheControl] = $"{visibility},max-age={duration}";
		if (location == ResponseCacheLocation.None)
		{
			response.Headers[HeaderNames.Pragma] = "no-cache";
		}
	}

	private static string GetAccessTokenFromAuthorizationHeader(HttpRequest request)
	{
		if (!request.Headers.TryGetValue(HeaderNames.Authorization, out var values))
		{
			return string.Empty;
		}

		var authenticationHeader = values.First();
		if (!AuthenticationHeaderValue.TryParse(authenticationHeader, out var authorization))
		{
			return string.Empty;
		}

		var scheme = authorization.Scheme;
		return authorization.Parameter == null ||
			   string.Compare(scheme, "Bearer", StringComparison.CurrentCultureIgnoreCase) != 0
			? string.Empty
			: authorization.Parameter;
	}

	private static async Task<string?> GetAccessTokenFromBodyParameterAsync(HttpRequest request)
	{
		if (!request.HasFormContentType)
		{
			return null;
		}

		var content = await request.ReadFormAsync().ConfigureAwait(false);
		return !content.TryGetValue(StandardAuthorizationResponseNames.AccessTokenName, out var result)
			? null
			: result.First();
	}

	private static string GetAccessTokenFromQueryString(HttpRequest request)
	{
		var accessTokenName = StandardAuthorizationResponseNames.AccessTokenName;
		var record = request.Query.FirstOrDefault(q => q.Key == accessTokenName);
		return record.Equals(default(KeyValuePair<string, StringValues>)) ? string.Empty : record.Value.First()!;
	}

	private static object? ConvertValue(Type propertyType, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
		if (actualType == typeof(string))
		{
			return value;
		}

		if (actualType == typeof(Uri))
		{
			return Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;
		}

		if (actualType == typeof(double))
		{
			return double.TryParse(
				value,
				NumberStyles.Float | NumberStyles.AllowThousands,
				CultureInfo.InvariantCulture,
				out var number)
				? number
				: null;
		}

		if (actualType.IsEnum)
		{
			return Enum.TryParse(actualType, value, true, out var enumValue) ? enumValue : null;
		}

		return null;
	}
}


