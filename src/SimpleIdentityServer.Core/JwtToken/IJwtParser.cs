namespace SimpleIdentityServer.Core.JwtToken
{
    using System.Threading.Tasks;
    using Shared;

    public interface IJwtParser
    {
        bool IsJweToken(string jwe);
        bool IsJwsToken(string jws);
        Task<string> DecryptAsync(string jwe);
        Task<string> DecryptAsync(string jwe, string clientId);
        Task<string> DecryptWithPasswordAsync(string jwe, string clientId, string password);
        Task<JwsPayload> UnSignAsync(string jws);
        Task<JwsPayload> UnSignAsync(string jws, string clientId);
    }
}