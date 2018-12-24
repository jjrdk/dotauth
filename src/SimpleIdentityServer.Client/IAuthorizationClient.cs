namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Shared.Requests;

    public interface IAuthorizationClient
    {
        Task<GetAuthorizationResult> ResolveAsync(string discoveryDocumentationUrl, AuthorizationRequest request);
    }
}