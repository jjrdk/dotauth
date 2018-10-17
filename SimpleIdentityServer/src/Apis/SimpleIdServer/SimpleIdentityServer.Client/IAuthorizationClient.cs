namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Core.Common.DTOs.Requests;
    using Results;

    public interface IAuthorizationClient
    {
        Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request);
    }
}