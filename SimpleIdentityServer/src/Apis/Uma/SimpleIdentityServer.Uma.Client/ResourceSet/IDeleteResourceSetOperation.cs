namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Core.Common;

    public interface IDeleteResourceSetOperation
    {
        Task<BaseResponse> ExecuteAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue);
    }
}