namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Results;

    public interface ISearchClaimsAction
    {
        Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter);
    }
}