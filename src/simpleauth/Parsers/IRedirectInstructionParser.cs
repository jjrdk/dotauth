namespace SimpleAuth.Parsers
{
    using Microsoft.AspNetCore.Routing;
    using Results;

    public interface IRedirectInstructionParser
    {
        ActionInformation GetActionInformation(RedirectInstruction action);

        RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction);
    }
}