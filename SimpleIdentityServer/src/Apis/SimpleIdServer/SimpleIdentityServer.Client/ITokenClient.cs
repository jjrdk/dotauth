namespace SimpleIdentityServer.Client
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface ITokenClient
    {
        Task<GetTokenResult> GetToken(Uri tokenUri);
        Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }
}