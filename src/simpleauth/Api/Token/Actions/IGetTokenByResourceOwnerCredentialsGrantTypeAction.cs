namespace SimpleAuth.Api.Token.Actions
{
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName);
    }
}