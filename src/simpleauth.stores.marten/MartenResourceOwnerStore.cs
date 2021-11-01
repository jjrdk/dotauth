namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Marten.Pagination;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Properties;

    /// <summary>
    /// Defines the Marten based resource owner repository.
    /// </summary>
    /// <seealso cref="IResourceOwnerRepository" />
    public class MartenResourceOwnerStore : IResourceOwnerRepository
    {
        private readonly string _salt;
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenResourceOwnerStore"/> class.
        /// </summary>
        /// <param name="salt">The hashing salt</param>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenResourceOwnerStore(string salt, Func<IDocumentSession> sessionFactory)
        {
            _salt = salt;
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<ResourceOwner?> GetResourceOwnerByClaim(
            string key,
            string value,
            CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var ro = await session.Query<ResourceOwner>()
                .FirstOrDefaultAsync(r => r.Claims.Any(x => x.Type == key && x.Value == value), token: cancellationToken)
                .ConfigureAwait(false);

            return ro;
        }

        /// <inheritdoc />
        public async Task<ResourceOwner?> Get(string id, CancellationToken cancellationToken = default)
        {
            await using var session = _sessionFactory();
            var ro = await session.LoadAsync<ResourceOwner>(id, cancellationToken).ConfigureAwait(false);

            return ro;
        }

        /// <inheritdoc />
        public async Task<ResourceOwner?> Get(ExternalAccountLink externalAccount, CancellationToken cancellationToken)
        {
            var externalAccountSubject = externalAccount.Subject;
            var externalAccountIssuer = externalAccount.Issuer;

            await using var session = _sessionFactory();
            var ro = await session.Query<ResourceOwner>()
                .Where(x => x.ExternalLogins.Any(e => e.Subject == externalAccountSubject))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return ro.SingleOrDefault(
                x => x.ExternalLogins.Any(
                    e => e.Subject == externalAccountSubject && e.Issuer == externalAccountIssuer));
        }

        /// <inheritdoc />
        public async Task<ResourceOwner?> Get(string id, string password, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var hashed = password.ToSha256Hash(_salt);
            var ro = await session.Query<ResourceOwner>()
                .FirstOrDefaultAsync(x => x.Subject == id && x.Password == hashed, cancellationToken)
                .ConfigureAwait(false);

            return ro;
        }

        /// <inheritdoc />
        public async Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var resourceOwners = await session.Query<ResourceOwner>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return resourceOwners.ToArray();
        }

        /// <inheritdoc />
        public async Task<bool> Insert(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            await using var session = _sessionFactory();
            resourceOwner.Password = resourceOwner.Password?.ToSha256Hash(_salt);
            session.Store(resourceOwner);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<Option> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var user = await session.LoadAsync<ResourceOwner>(resourceOwner.Subject, cancellationToken)
                .ConfigureAwait(false);
            if (user == null)
            {
                return new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = SharedStrings.TheRoDoesntExist,
                    Status = HttpStatusCode.InternalServerError
                };
            }

            user.IsLocalAccount = resourceOwner.IsLocalAccount;
            user.ExternalLogins = resourceOwner.ExternalLogins;
            user.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
            user.UpdateDateTime = DateTimeOffset.UtcNow;
            user.Claims = resourceOwner.Claims;
            session.Update(resourceOwner);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new Option.Success();
        }

        /// <inheritdoc />
        public async Task<bool> SetPassword(string subject, string password, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var user = await session.LoadAsync<ResourceOwner>(subject, cancellationToken)
                .ConfigureAwait(false);
            if (user == null)
            {
                return false;
            }

            user.Password = password.ToSha256Hash(_salt);
            session.Update(user);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Delete(string subject, CancellationToken cancellationToken = default)
        {
            await using var session = _sessionFactory();
            session.Delete<ResourceOwner>(subject);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<PagedResult<ResourceOwner>> Search(
            SearchResourceOwnersRequest parameter,
            CancellationToken cancellationToken = default)
        {
            await using var session = _sessionFactory();
            var subjects = parameter.Subjects ?? Array.Empty<string>();
            var results = await session.Query<ResourceOwner>()
                .Where(r => r.Claims.Any(x => x.Type == OpenIdClaimTypes.Subject && x.Value.IsOneOf(subjects)))
                .ToPagedListAsync(parameter.StartIndex + 1, parameter.NbResults, cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<ResourceOwner>
            {
                Content = results.ToArray(),
                TotalResults = results.TotalItemCount,
                StartIndex = parameter.StartIndex
            };
        }
    }
}
