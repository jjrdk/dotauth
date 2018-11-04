namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IGetTokenByRefreshTokenGrantTypeAction
    {
        Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }
}