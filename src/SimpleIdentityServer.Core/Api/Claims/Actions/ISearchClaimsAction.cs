namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System.Threading.Tasks;
    using Shared.Parameters;
    using Shared.Results;

    public interface ISearchClaimsAction
    {
        Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter);
    }
}