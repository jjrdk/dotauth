namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the token cache class.
    /// </summary>
    public sealed class TokenCache : ITokenCache
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly List<StoredToken> _tokens;
        private readonly TokenClient _client;
        private JsonWebKeySet _jsonWebKeySet;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCache"/> class.
        /// </summary>
        /// <param name="clientId">The id of the client application.</param>
        /// <param name="clientSecret">The secret of the client application.</param>
        /// <param name="discoveryDocumentUri">The <see cref="Uri"/> of the discovery document.</param>
        /// <param name="backChannelHandler">The request handler.</param>
        public TokenCache(
            string clientId,
            string clientSecret,
            Uri discoveryDocumentUri,
            HttpMessageHandler backChannelHandler = null)
        {
            _tokens = new List<StoredToken>();
            _client = new TokenClient(
                TokenCredentials.FromClientCredentials(clientId, clientSecret),
                new HttpClient(backChannelHandler ?? new HttpClientHandler()),
                discoveryDocumentUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCache"/> class.
        /// </summary>
        /// <param name="clientId">The id of the client application.</param>
        /// <param name="clientSecret">The secret of the client application.</param>
        /// <param name="discoveryDocument">The <see cref="DiscoveryInformation"/> document.</param>
        /// <param name="backChannelHandler">The request handler.</param>
        public TokenCache(
            string clientId,
            string clientSecret,
            DiscoveryInformation discoveryDocument,
            HttpMessageHandler backChannelHandler = null)
        {
            _tokens = new List<StoredToken>();
            _client = new TokenClient(
                TokenCredentials.FromClientCredentials(clientId, clientSecret),
                new HttpClient(backChannelHandler ?? new HttpClientHandler()),
                discoveryDocument);
        }

        /// <inheritdoc />
        public async Task<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                return _jsonWebKeySet ??= await _client.GetJwks().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<GrantedTokenResponse> GetToken(params string[] scopes)
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var allScopes = new HashSet<string>(scopes);

                var storedToken = _tokens.FirstOrDefault(t => allScopes.SetEquals(t.GrantedToken.Scope.Split(' ')));
                if (storedToken != null)
                {
                    if (DateTime.UtcNow < storedToken.Expiration)
                    {
                        return storedToken.GrantedToken;
                    }

                    _tokens.Remove(storedToken);
                }

                var tokenResponse = await _client.GetToken(TokenRequest.FromScopes(scopes)).ConfigureAwait(false);
                if (!tokenResponse.HasError)
                {
                    _tokens.Add(
                        new StoredToken(
                            tokenResponse.Content,
                            DateTimeOffset.UtcNow.AddSeconds(tokenResponse.Content.ExpiresIn)));
                }

                return tokenResponse.Content;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private class StoredToken
        {
            public StoredToken(GrantedTokenResponse tokenResponse, DateTimeOffset expiration)
            {
                Expiration = expiration;
                GrantedToken = tokenResponse;
            }

            public DateTimeOffset Expiration { get; }

            public GrantedTokenResponse GrantedToken { get; }
        }
    }
}
