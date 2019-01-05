namespace SimpleAuth.Helpers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGrantedTokenHelper 
    {
        Task<GrantedToken> GetValidGrantedTokenAsync(string scopes, string clientId, JwtPayload idTokenJwsPayload = null, JwtPayload userInfoJwsPayload = null);
    }
}