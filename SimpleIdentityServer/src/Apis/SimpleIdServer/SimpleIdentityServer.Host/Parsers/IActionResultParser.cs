namespace SimpleIdentityServer.Host.Parsers
{
    using Core.Results;
    using Microsoft.AspNetCore.Routing;

    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult);
        RouteValueDictionary GetRedirectionParameters(ActionResult actionResult);
    }
}