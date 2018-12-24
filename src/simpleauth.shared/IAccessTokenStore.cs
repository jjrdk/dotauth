namespace SimpleIdentityServer.Shared
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Responses;

    public interface IAccessTokenStore
    {
        /// <summary>
        /// Get an access token (grant_type = client credentials, client auth method = post secret).
        /// </summary>
        /// <param name="url"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes);
    }
}