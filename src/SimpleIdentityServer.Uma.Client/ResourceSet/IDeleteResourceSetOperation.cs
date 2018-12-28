namespace SimpleAuth.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public interface IDeleteResourceSetOperation
    {
        Task<BaseResponse> ExecuteAsync(
            string resourceSetId,
            string resourceSetUrl,
            string authorizationHeaderValue);
    }
}