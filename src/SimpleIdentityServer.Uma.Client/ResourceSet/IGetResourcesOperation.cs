namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetResourcesOperation
    {
        Task<GetResourcesResult> ExecuteAsync(string resourceSetUrl, string authorizationHeaderValue);
    }
}