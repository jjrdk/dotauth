namespace SimpleAuth.Helpers
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGrantedTokenGeneratorHelper
    {
        Task<GrantedToken> GenerateToken(string clientId, string scope, string issuerName, IDictionary<string, object> additionalClaims = null, JwtPayload userInformationPayload = null, JwtPayload idTokenPayload = null);
        Task<GrantedToken> GenerateToken(Client clientId, string scope, string issuerName, IDictionary<string, object> additionalClaims = null, JwtPayload userInformationPayload = null, JwtPayload idTokenPayload = null);
    }
}