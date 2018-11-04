namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Shared;

    public interface IDeleteResourceSetOperation
    {
        Task<BaseResponse> ExecuteAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue);
    }
}