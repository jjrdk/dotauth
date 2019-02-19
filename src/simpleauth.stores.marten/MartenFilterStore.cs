namespace SimpleAuth.Stores.Marten
{
    using System;
    using SimpleAuth.Shared.AccountFiltering;
    using SimpleAuth.Shared.Repositories;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;

    public class MartenFilterStore : IFilterStore
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        public MartenFilterStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var filters = await session.Query<Filter>().ToListAsync(token: cancellationToken).ConfigureAwait(false);
                return filters.ToArray();
            }
        }
    }
}