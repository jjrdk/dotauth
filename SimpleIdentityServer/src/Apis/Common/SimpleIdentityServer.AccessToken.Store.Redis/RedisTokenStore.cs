using SimpleIdentityServer.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store.Redis
{
    internal sealed class RedisTokenStore : BaseAccessTokenStore
    {
        private List<StoredToken> _storedTokens;
        private readonly RedisStorage _redisStorage;
        private readonly int _slidingExpirationTime;

        public RedisTokenStore(RedisStorage redisStorage, IIdentityServerClientFactory identityServerClientFactory, int slidingExpirationTime = 3600) : base(identityServerClientFactory)
        {
            _redisStorage = redisStorage;
            _slidingExpirationTime = slidingExpirationTime;
        }

        protected override async Task AddToken(StoredToken storedToken)
        {
            if(_storedTokens == null)
            {
                _storedTokens = new List<StoredToken>();
            }

            _storedTokens.Add(storedToken);
            await _redisStorage.SetAsync(storedToken.Url, _storedTokens, _slidingExpirationTime).ConfigureAwait(false);
        }

        protected override async Task<StoredToken> GetToken(string url, IEnumerable<string> scopes)
        {
            _storedTokens = await _redisStorage.TryGetValueAsync<List<StoredToken>>(url).ConfigureAwait(false);
            if (_storedTokens == null)
            {
                return null;
            }

            return _storedTokens.FirstOrDefault(st => scopes.All(s => st.Scopes.Contains(s)));
        }

        protected override void RemoveToken(StoredToken tk)
        {
            if (_storedTokens == null)
            {
                return;
            }

            _storedTokens.Remove(tk);
        }
    }
}
