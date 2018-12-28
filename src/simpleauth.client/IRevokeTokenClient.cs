namespace SimpleAuth.Client
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IRevokeTokenClient
    {
        Task<GetRevokeTokenResult> ExecuteAsync(Uri tokenUri);
        Task<GetRevokeTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }
}