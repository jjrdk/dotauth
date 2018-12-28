namespace SimpleAuth.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public interface IUpdatePolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(PutPolicy request, string url, string token);
    }
}