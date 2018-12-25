namespace SimpleIdentityServer.Core.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public interface IGrantedTokenGeneratorHelper
    {
        Task<GrantedToken> GenerateTokenAsync(string clientId, string scope, string issuerName, IDictionary<string, object> additionalClaims = null, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
        Task<GrantedToken> GenerateTokenAsync(Client clientId, string scope, string issuerName, IDictionary<string, object> additionalClaims = null, JwsPayload userInformationPayload = null, JwsPayload idTokenPayload = null);
    }
}