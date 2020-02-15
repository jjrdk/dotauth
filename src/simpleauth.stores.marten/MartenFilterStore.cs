namespace SimpleAuth.Stores.Marten
{
    using System;
    using SimpleAuth.Shared.Repositories;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the Marten based filter repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IFilterStore" />
    public class MartenFilterStore : IFilterStore
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenFilterStore"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenFilterStore(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
        {
            using var session = _sessionFactory();
            var filters = await session.Query<Filter>().ToListAsync(token: cancellationToken).ConfigureAwait(false);
            return filters.ToArray();
        }
    }
}