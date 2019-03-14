namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the marten based jwks repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IJwksRepository" />
    public class MartenJwksRepository : IJwksRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenConsentRepository"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenJwksRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<JsonWebKeySet> GetPublicKeys(CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var keysets = await session.Query<JsonWebKey>()
                    .Where(x => !x.HasPrivateKey)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                var jwks = ToSet(keysets);
                return jwks;
            }
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningKey(string alg, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var webKey = await session.Query<JsonWebKey>()
                    .FirstOrDefaultAsync(
                        x => x.Alg == alg && x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (webKey.X5c != null)
                {
                    foreach (var certString in webKey.X5c)
                    {
                        return new X509SigningCredentials(new X509Certificate2(Convert.FromBase64String(certString)));
                    }
                }

                return new SigningCredentials(webKey, alg);
            }
        }

        /// <inheritdoc />
        public async Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Store(key);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.DeleteWhere<JsonWebKey>(x => true);
                session.Store(keySet.Keys);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }
        }

        private static JsonWebKeySet ToSet(IEnumerable<JsonWebKey> keys)
        {
            var jwks = new JsonWebKeySet();
            foreach (var key in keys)
            {
                jwks.Keys.Add(key);
            }

            return jwks;
        }

    }
}