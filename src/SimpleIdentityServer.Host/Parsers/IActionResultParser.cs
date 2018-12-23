namespace SimpleIdentityServer.Host.Parsers
{
    using Core.Results;
    using Microsoft.AspNetCore.Routing;

    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(EndpointResult endpointResult);
        RouteValueDictionary GetRedirectionParameters(EndpointResult endpointResult);
    }
}