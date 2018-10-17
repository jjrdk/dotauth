namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Results;

    public interface IAddPolicyOperation
    {
        Task<AddPolicyResult> ExecuteAsync(PostPolicy request, string url, string authorizationHeaderValue);
    }
}