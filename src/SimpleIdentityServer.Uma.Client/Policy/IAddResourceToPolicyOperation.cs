namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Shared;

    public interface IAddResourceToPolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, PostAddResourceSet request, string url, string token);
    }
}