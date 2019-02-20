namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Results;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MartenResourceOwnerStore : IResourceOwnerRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        public MartenResourceOwnerStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

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

        public async Task<ResourceOwner> Get(string id, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var ro = await session.LoadAsync<ResourceOwner>(id, cancellationToken).ConfigureAwait(false);

                return ro;
            }
        }

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

        public async Task<ResourceOwner> Get(string id, string password, CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var hashed = password.ToSha256Hash();
                var ro = await session.Query<ResourceOwner>()
                    .FirstOrDefaultAsync(x => x.Id == id && x.Password == hashed)
                    .ConfigureAwait(false);

                return ro;
            }
        }

        public async Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken)
        {
            using (var session = _sessionFactory())
            {
                var resourceOwners = await session.Query<ResourceOwner>().ToListAsync(cancellationToken).ConfigureAwait(false);
                return resourceOwners.ToArray();
            }
        }

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

        public async Task<bool> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var user = await session.LoadAsync<ResourceOwner>(resourceOwner.Id, cancellationToken).ConfigureAwait(false);
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

        public async Task<bool> Delete(string subject, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete<ResourceOwner>(subject);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<SearchResourceOwnerResult> Search(
            SearchResourceOwnersRequest parameter,
            CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var subjects = parameter.Subjects;
                var results = await session.Query<ResourceOwner>()
                    .Where(
                        r => r.Claims.Any(
                            x => x.Type == OpenIdClaimTypes.Subject
                                 && x.Value.IsOneOf(subjects)))
                    .Skip(parameter.StartIndex)
                    .Take(parameter.NbResults)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return new SearchResourceOwnerResult
                {
                    Content = results,
                    TotalResults = results.Count,
                    StartIndex = parameter.StartIndex
                };
            }
        }
    }
}
