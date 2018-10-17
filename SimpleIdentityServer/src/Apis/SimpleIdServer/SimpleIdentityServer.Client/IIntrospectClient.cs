namespace SimpleIdentityServer.Client
{
    using System.Threading.Tasks;
    using Results;

    public interface IIntrospectClient
    {
        Task<GetIntrospectionResult> ResolveAsync(string discoveryDocumentationUrl);
    }
}