namespace SimpleAuth.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using Results;

    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(EndpointResult endpointResult);
        RouteValueDictionary GetRedirectionParameters(EndpointResult endpointResult);
    }
}