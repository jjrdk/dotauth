namespace SimpleIdentityServer.Host.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using SimpleAuth.Results;

    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(EndpointResult endpointResult);
        RouteValueDictionary GetRedirectionParameters(EndpointResult endpointResult);
    }
}