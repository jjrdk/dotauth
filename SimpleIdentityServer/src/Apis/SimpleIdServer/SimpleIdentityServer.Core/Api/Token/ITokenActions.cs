namespace SimpleIdentityServer.Core.Api.Token
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Common.Models;
    using Parameters;

    public interface ITokenActions
    {
        Task<GrantedToken> GetTokenByResourceOwnerCredentialsGrantType(ResourceOwnerGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
        Task<GrantedToken> GetTokenByAuthorizationCodeGrantType(AuthorizationCodeGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
        Task<GrantedToken> GetTokenByRefreshTokenGrantType(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
        Task<GrantedToken> GetTokenByClientCredentialsGrantType(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
        Task<bool> RevokeToken(RevokeTokenParameter revokeTokenParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }
}