namespace SimpleAuth.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;

    public interface IAddResourceSetOperation
    {
        Task<AddResourceSetResult> ExecuteAsync(PostResourceSet request, string url, string token);
    }
}