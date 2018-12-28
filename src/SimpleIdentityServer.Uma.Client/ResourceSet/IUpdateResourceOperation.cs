namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface IUpdateResourceOperation
    {
        Task<UpdateResourceSetResult> ExecuteAsync(PutResourceSet request, string url, string token);
    }
}