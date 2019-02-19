namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Marten based resource owner repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IResourceOwnerRepository" />
    public class MartenResourceOwnerStore : IResourceOwnerRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenResourceOwnerStore"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenResourceOwnerStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<ResourceOwner> GetResourceOwnerByClaim(
            string key,
            string value,
            CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var ro = await session.Query<ResourceOwner>()
                    .FirstOrDefaultAsync(r => r.Claims.Any(x => x.Type == key && x.Value == value))
                    .ConfigureAwait(false);

                return ro;
            }
        }

        /// <inheritdoc />
        public async Task<ResourceOwner> Get(string id, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var ro = await session.LoadAsync<ResourceOwner>(id, cancellationToken).ConfigureAwait(false);

                return ro;
            }
        }

        /// <inheritdoc />
        public async Task<ResourceOwner> Get(ExternalAccountLink externalAccount, CancellationToken cancellationToken)
        {
            if (externalAccount == null)
            {
                throw new ArgumentNullException(nameof(externalAccount));
            }

            using (var session = _sessionFactory())
            {
                var ro = await session.Query<ResourceOwner>()
                    .Where(
                        x => x.ExternalLogins != null
                             && x.ExternalLogins.Any(
                                 e => e.Subject == externalAccount.Subject && e.Issuer == externalAccount.Issuer))
                    .SingleOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

                return ro;
            }
        }

        /// <inheritdoc />
        public async Task<ResourceOwner> Get(string id, string password, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var hashed = password.ToSha256Hash();
                var ro = await session.Query<ResourceOwner>()
                    .FirstOrDefaultAsync(x => x.Subject == id && x.Password == hashed)
                    .ConfigureAwait(false);

                return ro;
            }
        }

        /// <inheritdoc />
        public async Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var resourceOwners = await session.Query<ResourceOwner>()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                return resourceOwners.ToArray();
            }
        }

        /// <inheritdoc />
        public async Task<bool> Insert(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                resourceOwner.Password = resourceOwner.Password.ToSha256Hash();
                session.Store(resourceOwner);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var user = await session.LoadAsync<ResourceOwner>(resourceOwner.Subject, cancellationToken)
                    .ConfigureAwait(false);
                if (user == null)
                {
                    return false;
                }

                user.IsLocalAccount = resourceOwner.IsLocalAccount;
                user.ExternalLogins = resourceOwner.ExternalLogins;
                user.Password = resourceOwner.Password.ToSha256Hash();
                user.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
                user.UpdateDateTime = DateTime.UtcNow;
                user.Claims = resourceOwner.Claims;
                session.Update(resourceOwner);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Delete(string subject, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete<ResourceOwner>(subject);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        /// <inheritdoc />
        public async Task<GenericResult<ResourceOwner>> Search(
            SearchResourceOwnersRequest parameter,
            CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var subjects = parameter.Subjects;
                var results = await session.Query<ResourceOwner>()
                    .Where(r => r.Claims.Any(x => x.Type == OpenIdClaimTypes.Subject && x.Value.IsOneOf(subjects)))
                    .Skip(parameter.StartIndex)
                    .Take(parameter.NbResults)
                    .ToListAsync(token: cancellationToken)
                    .ConfigureAwait(false);

                return new GenericResult<ResourceOwner>
                {
                    Content = results.ToArray(), TotalResults = results.Count, StartIndex = parameter.StartIndex
                };
            }
        }
    }
}
