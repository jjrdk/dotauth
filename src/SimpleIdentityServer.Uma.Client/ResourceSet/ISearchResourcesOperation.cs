namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface ISearchResourcesOperation
    {
        Task<SearchResourceSetResult> ExecuteAsync(string url, SearchResourceSet parameter, string authorizationHeaderValue = null);
    }
}