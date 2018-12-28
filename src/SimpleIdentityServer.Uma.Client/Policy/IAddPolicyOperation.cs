namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IAddPolicyOperation
    {
        Task<AddPolicyResult> ExecuteAsync(PostPolicy request, string url, string authorizationHeaderValue);
    }
}