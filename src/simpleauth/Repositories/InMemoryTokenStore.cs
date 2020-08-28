namespace SimpleAuth.Repositories
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class InMemoryTokenStore : ITokenStore, ICleanable
    {
        private readonly List<GrantedToken> _tokens = new List<GrantedToken>();

        public Task<GrantedToken?> GetToken(
            string scopes,
            string clientId,
            JwtPayload? idTokenJwsPayload,
            JwtPayload? userInfoJwsPayload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_tokens.Count == 0)
            {
                return Task.FromResult<GrantedToken?>(null);
            }

            var grantedTokens = _tokens
                .Where(g => g.Scope == scopes && g.ClientId == clientId)
                .OrderByDescending(g => g.CreateDateTime);
            if (!_tokens.Any())
            {
                return Task.FromResult<GrantedToken?>(null);
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

                return Task.FromResult<GrantedToken?>(grantedToken);
            }

            return Task.FromResult<GrantedToken?>(null);
        }

        public Task<GrantedToken?> GetRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            var grantedToken = _tokens.FirstOrDefault(x => x.RefreshToken == refreshToken);

            return Task.FromResult(grantedToken);
        }

        public Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            var grantedToken = _tokens.FirstOrDefault(x => x.AccessToken == accessToken);

            return Task.FromResult(grantedToken);
        }

        public Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
        {
            _tokens.Add(grantedToken);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            var removed = _tokens.RemoveAll(x => x.RefreshToken == refreshToken);
            return Task.FromResult(removed > 0);
        }

        public Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            var removed = _tokens.RemoveAll(x => x.AccessToken == accessToken);
            return Task.FromResult(removed > 0);
        }

        public Task Clean(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _tokens.Clear();
            return Task.CompletedTask;
        }

        private static bool CompareJwsPayload(JwtPayload firstJwsPayload, JwtPayload secondJwsPayload)
        {
            return firstJwsPayload.All(secondJwsPayload.Contains);
        }
    }
}
