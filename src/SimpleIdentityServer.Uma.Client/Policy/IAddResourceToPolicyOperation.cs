namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IAddResourceToPolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, PostAddResourceSet request, string url, string token);
    }
}