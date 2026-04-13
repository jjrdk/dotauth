namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Allows optional feature assemblies to contribute UI endpoints to the DotAuth route surface.
/// </summary>
public interface IDotAuthUiEndpointRegistration
{
    /// <summary>
    /// Maps the feature's UI endpoints.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

