namespace SimpleIdentityServer.Host.Parsers
{
    using Core.Results;
    using Microsoft.AspNetCore.Routing;

    public interface IRedirectInstructionParser
    {
        ActionInformation GetActionInformation(RedirectInstruction action);

        RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction);
    }
}