namespace SimpleAuth.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;

    public interface IUpdateResourceOperation
    {
        Task<UpdateResourceSetResult> ExecuteAsync(PutResourceSet request, string url, string token);
    }
}