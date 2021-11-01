namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using global::Marten.Pagination;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Properties;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the marten based resource set repository.
    /// </summary>
    /// <seealso cref="IResourceSetRepository" />
    public class MartenResourceSetRepository : IResourceSetRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public MartenResourceSetRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public async Task<PagedResult<ResourceSet>> Search(
            SearchResourceSet parameter,
            CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();

            var parameterIds = parameter.Ids;
            var parameterNames = parameter.Names;
            var results = await session.Query<OwnedResourceSet>()
                .Where(x => x.Name.IsOneOf(parameterIds) && x.Type.IsOneOf(parameterNames))
                .ToPagedListAsync(parameter.StartIndex + 1, parameter.TotalResults, cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<ResourceSet>
            {
                Content = results.Select(x => x.AsResourceSet()).ToArray(),
                StartIndex = parameter.StartIndex,
                TotalResults = results.TotalItemCount
            };
        }

        /// <inheritdoc />
        public async Task<bool> Add(string owner, ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            session.Store(OwnedResourceSet.FromResourceSet(resourceSet, owner));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var resourceSet = await session.LoadAsync<OwnedResourceSet>(id, cancellationToken).ConfigureAwait(false);

            return resourceSet?.Owner == owner
                ? resourceSet.AsResourceSet()
                : null;
        }

        /// <inheritdoc />
        public async Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
        {
            await using var session = _sessionFactory();
            var resourceSets = await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);
            var owners = resourceSets.Select(x => x.Owner).Distinct();

            return owners.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var existing = await session.LoadAsync<OwnedResourceSet>(resourceSet.Id, cancellationToken).ConfigureAwait(false);
            if (existing == null)
            {
                return new ErrorDetails
                {
                    Status = HttpStatusCode.NotFound,
                    Title = ErrorCodes.NotUpdated,
                    Detail = SharedStrings.ResourceCannotBeUpdated
                };
            }
            session.Update(OwnedResourceSet.FromResourceSet(resourceSet, existing.Owner));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new Option.Success();
        }

        /// <inheritdoc />
        public async Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            var resourceSets = await session.Query<OwnedResourceSet>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return resourceSets.Select(x => x.AsResourceSet()).ToArray();
        }

        /// <inheritdoc />
        public async Task<bool> Remove(string id, CancellationToken cancellationToken)
        {
            await using var session = _sessionFactory();
            session.Delete<OwnedResourceSet>(id);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids)
        {
            await using var session = _sessionFactory();
            var resourceSets =
                await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);

            return resourceSets.Select(x => x.AsResourceSet()).ToArray();
        }
    }
}
