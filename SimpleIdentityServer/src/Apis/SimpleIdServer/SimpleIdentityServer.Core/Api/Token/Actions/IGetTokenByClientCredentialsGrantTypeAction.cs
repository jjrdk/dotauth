namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Common.Models;
    using Parameters;

    public interface IGetTokenByClientCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }
}