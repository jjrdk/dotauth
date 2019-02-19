namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MartenTokenStore : ITokenStore
    {
        private readonly Func<IDocumentSession> _sessionFactory;
        private readonly IEventPublisher _eventPublisher;

        public MartenTokenStore(Func<IDocumentSession> sessionFactory, IEventPublisher eventPublisher)
        {
            _sessionFactory = sessionFactory;
            _eventPublisher = eventPublisher;
        }

        public async Task<GrantedToken> GetToken(
            string scopes,
            string clientId,
            JwtPayload idTokenJwsPayload,
            JwtPayload userInfoJwsPayload,
            CancellationToken cancellationToken)
        {
            if (idTokenJwsPayload == null || userInfoJwsPayload == null)
            {
                return null;
            }

            using (var session = _sessionFactory())
            {
                var options = await session.Query<GrantedToken>()
                    .Where(
                        x => x.ClientId == clientId
                             && x.Scope == scopes
                             && x.IdTokenPayLoad != null
                             && x.UserInfoPayLoad != null)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return options.FirstOrDefault(x => idTokenJwsPayload.All(x.IdTokenPayLoad.Contains) && userInfoJwsPayload.All(x.UserInfoPayLoad.Contains));
            }
        }

        public async Task<GrantedToken> GetRefreshToken(string getRefreshToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var grantedToken = await session.Query<GrantedToken>()
                    .FirstOrDefaultAsync(x => x.RefreshToken == getRefreshToken)
                    .ConfigureAwait(false);
                return grantedToken;
            }
        }

        public Task<GrantedToken> GetAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                session.Store(grantedToken);
                await session.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                session.DeleteWhere<GrantedToken>(x => x.AccessToken == accessToken);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                session.DeleteWhere<GrantedToken>(x => x.RefreshToken == refreshToken);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
    }
}
