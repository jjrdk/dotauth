namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Results;

    public interface ISearchResourcesOperation
    {
        Task<SearchResourceSetResult> ExecuteAsync(string url, SearchResourceSet parameter, string authorizationHeaderValue = null);
    }
}