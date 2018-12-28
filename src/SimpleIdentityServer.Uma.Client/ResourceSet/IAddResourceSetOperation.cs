namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IAddResourceSetOperation
    {
        Task<AddResourceSetResult> ExecuteAsync(PostResourceSet request, string url, string token);
    }
}