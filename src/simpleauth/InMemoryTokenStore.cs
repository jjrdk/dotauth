namespace SimpleAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shared;
    using Shared.Models;

    internal sealed class InMemoryTokenStore : ITokenStore
    {
        private readonly Dictionary<string, GrantedToken> _tokens;
        private readonly Dictionary<string, string> _mappingStrToRefreshTokens;
        private readonly Dictionary<string, string> _mappingStrToAccessTokens;

        public InMemoryTokenStore()
        {
            _tokens = new Dictionary<string, GrantedToken>();
            _mappingStrToRefreshTokens = new Dictionary<string, string>();
            _mappingStrToAccessTokens = new Dictionary<string, string>();
        }

        public Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload)
        {
            if (_tokens == null || !_tokens.Any())
            {
                return Task.FromResult((GrantedToken)null);
            }

            var grantedTokens = _tokens.Values
                .Where(g => g.Scope == scopes && g.ClientId == clientId)
                .OrderByDescending(g => g.CreateDateTime);
            if (!_tokens.Any())
            {
                return Task.FromResult((GrantedToken)null);
            }

            foreach (var grantedToken in grantedTokens)
            {
                if (grantedToken.IdTokenPayLoad != null || idTokenJwsPayload != null)
                {
                    if (grantedToken.IdTokenPayLoad == null || idTokenJwsPayload == null)
                    {
                        continue;
                    }

                    if (!CompareJwsPayload(idTokenJwsPayload, grantedToken.IdTokenPayLoad))
                    {
                        continue;
                    }
                }

                if (grantedToken.UserInfoPayLoad != null || userInfoJwsPayload != null)
                {
                    if (grantedToken.UserInfoPayLoad == null || userInfoJwsPayload == null)
                    {
                        continue;
                    }

                    if (!CompareJwsPayload(userInfoJwsPayload, grantedToken.UserInfoPayLoad))
                    {
                        continue;
                    }
                }

                return Task.FromResult(grantedToken);
            }

            return Task.FromResult((GrantedToken)null);
        }

        public Task<GrantedToken> GetRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!_mappingStrToRefreshTokens.ContainsKey(refreshToken))
            {
                return Task.FromResult((GrantedToken)null);
            }

            return Task.FromResult(_tokens[_mappingStrToRefreshTokens[refreshToken]]);
        }

        public Task<GrantedToken> GetAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!_mappingStrToAccessTokens.ContainsKey(accessToken))
            {
                return Task.FromResult((GrantedToken)null);
            }

            return Task.FromResult(_tokens[_mappingStrToAccessTokens[accessToken]]);
        }

        public Task<bool> AddToken(GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            if (_mappingStrToRefreshTokens.ContainsKey(grantedToken.RefreshToken)
                || _mappingStrToAccessTokens.ContainsKey(grantedToken.AccessToken))
            {
                return Task.FromResult(false);
            }

            var id = Guid.NewGuid().ToString();
            _tokens.Add(id, grantedToken);
            _mappingStrToRefreshTokens.Add(grantedToken.RefreshToken, id);
            _mappingStrToAccessTokens.Add(grantedToken.AccessToken, id);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!_mappingStrToRefreshTokens.ContainsKey(refreshToken))
            {
                return Task.FromResult(false);
            }

            _mappingStrToRefreshTokens.Remove(refreshToken);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (!_mappingStrToAccessTokens.ContainsKey(accessToken))
            {
                return Task.FromResult(false);
            }

            _mappingStrToAccessTokens.Remove(accessToken);
            return Task.FromResult(true);
        }

        public Task<bool> Clean()
        {
            _tokens.Clear();
            _mappingStrToAccessTokens.Clear();
            _mappingStrToRefreshTokens.Clear();
            return Task.FromResult(true);
        }

        private static bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            return firstJwsPayload.All(secondJwsPayload.Contains);
        }
    }
}
