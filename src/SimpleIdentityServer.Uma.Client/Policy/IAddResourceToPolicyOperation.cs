namespace SimpleAuth.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public interface IAddResourceToPolicyOperation
    {
        Task<BaseResponse> ExecuteAsync(string id, PostAddResourceSet request, string url, string token);
    }
}