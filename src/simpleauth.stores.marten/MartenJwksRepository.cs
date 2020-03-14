namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using System;
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
            using var session = _sessionFactory();
            var keysets = await session.Query<JsonWebKey>()
                .Where(x => !x.HasPrivateKey)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var jwks = keysets.ToSet();
            return jwks;
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningKey(string alg, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var webKeys = await session.Query<JsonWebKey>()
                .Where(x => x.Alg == alg && x.Use == JsonWebKeyUseNames.Sig)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

                var webKey = webKeys.First(x => x.HasPrivateKey && x.KeyOps.Contains(KeyOperations.Sign));

            if (webKey.X5c != null)
            {
                foreach (var certString in webKey.X5c)
                {
                    return new X509SigningCredentials(new X509Certificate2(Convert.FromBase64String(certString)));
                }
            }

            return new SigningCredentials(webKey, alg);
        }

        public async Task<SecurityKey> GetEncryptionKey(string alg, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var webKeys = await session.Query<JsonWebKey>()
                .Where(
                    x => x.HasPrivateKey && x.Alg == alg && x.Use == JsonWebKeyUseNames.Enc)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var webKey = webKeys.First(x => x.KeyOps.Contains(KeyOperations.Encrypt));

            if (webKey.X5c != null)
            {
                foreach (var certString in webKey.X5c)
                {
                    return new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(certString)));
                }
            }

            return webKey;
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetDefaultSigningKey(CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var webKeys = await session.Query<JsonWebKey>()
                .Where(x => x.Use == JsonWebKeyUseNames.Sig)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var webKey = webKeys.OrderBy(x => x.KeyId).First(x => x.KeyOps.Contains(KeyOperations.Sign));

            if (webKey.X5c != null)
            {
                foreach (var certString in webKey.X5c)
                {
                    return new X509SigningCredentials(new X509Certificate2(Convert.FromBase64String(certString)));
                }
            }

            return new SigningCredentials(webKey, webKey.Alg);
        }

        /// <inheritdoc />
        public async Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            session.Store(key);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            foreach (var key in keySet.Keys)
            {
                session.Delete<JsonWebKey>(key.KeyId);
            }

            session.Store(keySet.Keys);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
