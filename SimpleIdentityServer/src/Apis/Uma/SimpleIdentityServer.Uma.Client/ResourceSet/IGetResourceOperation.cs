namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;

    public interface IGetResourceOperation
    {
        Task<GetResourceSetResult> ExecuteAsync(string resourceSetId, string resourceSetUrl, string authorizationHeaderValue);
    }
}