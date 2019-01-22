namespace SimpleAuth.JwtToken
{
    using Parameters;
    using Shared.Models;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IJwtGenerator
    {
        JwtPayload UpdatePayloadDate(JwtPayload jwsPayload, Client client);
        JwtSecurityToken GenerateAccessToken(
            Client client,
            IEnumerable<string> scopes,
            string issuerName,
            params Claim[] additionalClaims);
        Task<JwtPayload> GenerateIdTokenPayloadForScopesAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter, string issuerName);
        Task<JwtPayload> GenerateFilteredIdTokenPayloadAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter, List<ClaimParameter> claimParameters, string issuerName);
        Task<JwtPayload> GenerateUserInfoPayloadForScopeAsync(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter);
        JwtPayload GenerateFilteredUserInfoPayload(List<ClaimParameter> claimParameters, ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter);
        void FillInOtherClaimsIdentityTokenPayload(JwtPayload jwsPayload, string authorizationCode, string accessToken, Client client);
        //Task<string> SignAsync(JwtPayload jwsPayload, string alg);
        //Task<string> EncryptAsync(JwtPayload jwe, string jweAlg, string jweEnc);
    }
}