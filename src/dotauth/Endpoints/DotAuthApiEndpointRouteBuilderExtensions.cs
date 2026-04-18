namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Endpoint mappings for the non-UI DotAuth protocol surface.
/// </summary>
public static class DotAuthApiEndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps the non-UI DotAuth protocol endpoints.
	/// This method delegates to more focused extension methods so the route
	/// mapping logic is separated by concern (discovery, jwks, token, etc.).
	/// </summary>
	/// <param name="endpoints">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static IEndpointRouteBuilder MapDotAuthApiEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapDiscoveryEndpoints();
		endpoints.MapJwksEndpoints();
		endpoints.MapTokenEndpoints();
		endpoints.MapAuthorizationEndpoints();
		endpoints.MapIntrospectionEndpoints();
		endpoints.MapUserInfoEndpoints();
		endpoints.MapSessionEndpoints();
		endpoints.MapClientsEndpoints();
		endpoints.MapScopesEndpoints();
		endpoints.MapResourceOwnersEndpoints();
		endpoints.MapResourceSetEndpoints();
		endpoints.MapPermissionsEndpoints();
		endpoints.MapDeviceAuthorizationEndpoints();
		endpoints.MapUmaConfigurationEndpoints();

		return endpoints;
	}
}



