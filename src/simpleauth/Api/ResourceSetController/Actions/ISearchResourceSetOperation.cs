namespace SimpleAuth.Api.ResourceSetController.Actions
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface ISearchResourceSetOperation
    {
        Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter);
    }
}