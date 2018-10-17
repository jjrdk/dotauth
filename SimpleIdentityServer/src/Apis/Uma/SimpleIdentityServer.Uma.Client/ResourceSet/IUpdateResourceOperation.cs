namespace SimpleIdentityServer.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Results;

    public interface IUpdateResourceOperation
    {
        Task<UpdateResourceSetResult> ExecuteAsync(PutResourceSet request, string url, string token);
    }
}