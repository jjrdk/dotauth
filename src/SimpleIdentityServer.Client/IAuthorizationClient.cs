namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;
    using Shared.Requests;

    public interface IAuthorizationClient
    {
        Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request);
    }
}