namespace SimpleIdentityServer.Uma.Core.Api.Token
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Parameters;
    using SimpleIdentityServer.Core.Common.Models;

    public interface IUmaTokenActions
    {
        Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName);
    }
}