namespace SimpleAuth.Client
{
    using System.Threading.Tasks;
    using Results;

    public interface ITokenClient
    {
        Task<GetTokenResult> ResolveAsync(string discoveryDocumentationUrl);
    }
}