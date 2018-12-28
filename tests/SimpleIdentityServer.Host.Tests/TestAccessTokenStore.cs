namespace SimpleAuth.Server.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Responses;

    internal class TestAccessTokenStore : IAccessTokenStore
    {
        public Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            return Task.FromResult(new GrantedTokenResponse());
        }
    }
}