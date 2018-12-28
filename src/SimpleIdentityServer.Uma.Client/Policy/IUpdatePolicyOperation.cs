namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IUpdatePolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(PutPolicy request, string url, string token);
    }
}