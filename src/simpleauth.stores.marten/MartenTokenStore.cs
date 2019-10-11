namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Marten based token store.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.ITokenStore" />
    public class MartenTokenStore : ITokenStore
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenTokenStore"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenTokenStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
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
                    .ToListAsync(token: cancellationToken)
                    .ConfigureAwait(false);
                return options.FirstOrDefault(x =>
                    idTokenJwsPayload.All(x.IdTokenPayLoad.Contains) &&
                    userInfoJwsPayload.All(x.UserInfoPayLoad.Contains));
            }
        }

        /// <inheritdoc />
        public async Task<GrantedToken> GetRefreshToken(string getRefreshToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var grantedToken = await session.Query<GrantedToken>()
                    .FirstOrDefaultAsync(x => x.RefreshToken == getRefreshToken, token: cancellationToken)
                    .ConfigureAwait(false);
                return grantedToken;
            }
        }

        /// <inheritdoc />
        public async Task<GrantedToken> GetAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var grantedToken = await session.Query<GrantedToken>()
                    .FirstOrDefaultAsync(x => x.AccessToken == accessToken, token: cancellationToken)
                    .ConfigureAwait(false);
                return grantedToken;
            }
        }

        /// <inheritdoc />
        public async Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                session.Store(grantedToken);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                session.DeleteWhere<GrantedToken>(x => x.AccessToken == accessToken);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
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
