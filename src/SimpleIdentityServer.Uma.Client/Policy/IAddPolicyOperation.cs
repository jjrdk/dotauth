namespace SimpleAuth.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;

    public interface IAddPolicyOperation
    {
        Task<AddPolicyResult> ExecuteAsync(PostPolicy request, string url, string authorizationHeaderValue);
    }
}