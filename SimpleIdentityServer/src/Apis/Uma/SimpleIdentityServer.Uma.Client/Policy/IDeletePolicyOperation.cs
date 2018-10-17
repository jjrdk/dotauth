namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Core.Common;

    public interface IDeletePolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, string url, string token);
    }
}