namespace SimpleAuth.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using Results;

    public interface IActionResultParser
    {
        RouteValueDictionary GetRedirectionParameters(EndpointResult endpointResult);
    }
}