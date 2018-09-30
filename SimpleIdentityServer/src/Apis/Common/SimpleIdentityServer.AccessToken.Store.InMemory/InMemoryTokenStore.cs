using SimpleIdentityServer.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store.InMemory
{
    public sealed class InMemoryTokenStore : BaseAccessTokenStore
    {
        private readonly List<StoredToken> _tokens;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public InMemoryTokenStore(IIdentityServerClientFactory identityServerClientFactory) : base(identityServerClientFactory)
        {
            _tokens = new List<StoredToken>();
            _identityServerClientFactory = identityServerClientFactory;
        }

        public List<StoredToken> Tokens
        {
            get
            {
                return _tokens;
            }
        }
               
        protected override Task AddToken(StoredToken storedToken)
        {
            _tokens.Add(storedToken);
            return Task.FromResult(0);
        }

        protected override Task<StoredToken> GetToken(string url, IEnumerable<string> scopes)
        {
            return Task.FromResult(_tokens.FirstOrDefault(t => t.Url == url && scopes.All(s => t.Scopes.Contains(s))));
        }

        protected override void RemoveToken(StoredToken token)
        {
            _tokens.Remove(token);
        }
    }
}
