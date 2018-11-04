namespace SimpleIdentityServer.Core.JwtToken
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Shared;
    using Shared.Models;

    public interface IJwtGenerator
    {
        Task<JwsPayload> UpdatePayloadDate(JwsPayload jwsPayload);
        Task<JwsPayload> GenerateAccessToken(Client client, IEnumerable<string> scopes, string issuerName, IDictionary<string, object> additionalClaims);
        Task<JwsPayload> GenerateIdTokenPayloadForScopesAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter, string issuerName);
        Task<JwsPayload> GenerateFilteredIdTokenPayloadAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter, List<ClaimParameter> claimParameters, string issuerName);
        Task<JwsPayload> GenerateUserInfoPayloadForScopeAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter);
        JwsPayload GenerateFilteredUserInfoPayload(List<ClaimParameter> claimParameters, ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter);
        void FillInOtherClaimsIdentityTokenPayload(JwsPayload jwsPayload, string authorizationCode, string accessToken, Client client);
        Task<string> SignAsync(JwsPayload jwsPayload, JwsAlg alg);
        Task<string> EncryptAsync(string jwe, JweAlg jweAlg, JweEnc jweEnc);
    }
}