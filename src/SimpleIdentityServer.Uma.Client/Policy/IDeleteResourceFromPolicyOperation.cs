namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Shared;

    public interface IDeleteResourceFromPolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, string resourceId, string url, string token);
    }
}