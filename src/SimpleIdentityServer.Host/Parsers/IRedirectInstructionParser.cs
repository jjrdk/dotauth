namespace SimpleIdentityServer.Host.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using SimpleAuth.Results;

    public interface IRedirectInstructionParser
    {
        ActionInformation GetActionInformation(RedirectInstruction action);

        RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction);
    }
}