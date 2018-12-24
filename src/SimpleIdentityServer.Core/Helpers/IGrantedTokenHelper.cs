namespace SimpleIdentityServer.Core.Helpers
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public interface IGrantedTokenHelper 
    {
        Task<GrantedToken> GetValidGrantedTokenAsync(string scopes, string clientId, JwsPayload idTokenJwsPayload = null, JwsPayload userInfoJwsPayload = null);
    }
}