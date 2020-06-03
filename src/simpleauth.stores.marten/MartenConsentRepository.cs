namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    /// <summary>
    /// Defines the Marten based consent repository.
    /// </summary>
    /// <seealso cref="IConsentRepository" />
    public class MartenConsentRepository : IConsentRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenConsentRepository"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenConsentRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
            string subject,
            CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var consents = await session.Query<Consent>()
                .Where(x => x.Subject == subject)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return consents;
        }

        /// <inheritdoc />
        public async Task<bool> Insert(Consent record, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            session.Store(record);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> Delete(Consent record, CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            session.Delete(record.Id);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}