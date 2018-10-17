namespace SimpleIdentityServer.Core.Helpers
{
    using System.Threading.Tasks;
    using Common;
    using Common.Models;

    public interface IGrantedTokenGeneratorHelper
    {
        Task<GrantedToken> GenerateTokenAsync(string clientId, string scope, string issuerName, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
        Task<GrantedToken> GenerateTokenAsync(Client clientId, string scope, string issuerName, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
    }
}