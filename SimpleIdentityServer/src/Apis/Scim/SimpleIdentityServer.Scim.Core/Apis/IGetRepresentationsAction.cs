namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Parsers;
    using Results;

    public interface IGetRepresentationsAction
    {
        Task<ApiActionResult> Execute(string resourceType, SearchParameter searchParameter, string locationPattern);
    }
}