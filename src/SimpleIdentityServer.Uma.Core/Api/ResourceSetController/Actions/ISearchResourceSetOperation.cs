namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface ISearchResourceSetOperation
    {
        Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter);
    }
}