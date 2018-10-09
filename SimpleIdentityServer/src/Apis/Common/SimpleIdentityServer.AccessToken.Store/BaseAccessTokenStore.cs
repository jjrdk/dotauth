using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store
{
    public abstract class BaseAccessTokenStore : IAccessTokenStore
    {
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public BaseAccessTokenStore(IIdentityServerClientFactory identityServerClientFactory)
        {
            _identityServerClientFactory = identityServerClientFactory;
        }

        public async Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var token = await GetToken(url, scopes).ConfigureAwait(false);
            if (token != null)
            {
                if (DateTime.UtcNow < token.ExpirationDateTime)
                {
                    return token.GrantedToken;
                }

                RemoveToken(token);
            }

            var grantedToken = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret)
                .UseClientCredentials(scopes.ToArray())
                .ResolveAsync(url)
                .ConfigureAwait(false);
            var storedToken = new StoredToken
            {
                GrantedToken = grantedToken.Content,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(grantedToken.Content.ExpiresIn),
                Scopes = scopes,
                Url = url
            };
            await AddToken(storedToken).ConfigureAwait(false);
            return grantedToken.Content;
        }

        protected abstract Task<StoredToken> GetToken(string url, IEnumerable<string> scopes);
        protected abstract Task AddToken(StoredToken storedToken);
        protected abstract void RemoveToken(StoredToken token);
    }
}
