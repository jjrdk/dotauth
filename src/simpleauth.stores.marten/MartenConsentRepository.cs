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

    public class MartenConsentRepository : IConsentRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        public MartenConsentRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
            string subject,
            CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var consents = await session.Query<Consent>()
                    .Where(x => x.ResourceOwner.Id == subject)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                return consents;
            }
        }

        public async Task<bool> Insert(Consent record, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Store(record);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }
        }

        public async Task<bool> Delete(Consent record, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete(record.Id);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return true;
            }
        }
    }
}