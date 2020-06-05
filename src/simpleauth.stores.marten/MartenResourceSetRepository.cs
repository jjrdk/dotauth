namespace SimpleAuth.Stores.Marten
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using global::Marten.Pagination;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the marten based resource set repository.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Repositories.IResourceSetRepository" />
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
            using var session = _sessionFactory();
            parameter.Ids ??= Array.Empty<string>();
            parameter.Names ??= Array.Empty<string>();
            var results = await session.Query<OwnedResourceSet>()
                .Where(x => x.Name.IsOneOf(parameter.Ids) && x.Type.IsOneOf(parameter.Names))
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
            using var session = _sessionFactory();
            session.Store(OwnedResourceSet.FromResourceSet(resourceSet, owner));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ResourceSet> Get(string owner, string id, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            var resourceSet = await session.LoadAsync<OwnedResourceSet>(id, cancellationToken).ConfigureAwait(false);

            return resourceSet != null && resourceSet.Owner == owner
                ? resourceSet.AsResourceSet()
                : null;
        }

        /// <inheritdoc />
        public async Task<string> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
        {
            using var session = _sessionFactory();
            var resourceSets = await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids);
            var owners = resourceSets.Select(x => x.Owner).Distinct();

            return owners.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<bool> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            var existing = await session.LoadAsync<OwnedResourceSet>(resourceSet.Id, cancellationToken);
            if (existing == null)
            {
                return false;
            }
            session.Update(OwnedResourceSet.FromResourceSet(resourceSet, existing.Owner));
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            var resourceSets = await session.Query<OwnedResourceSet>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return resourceSets.Select(x => x.AsResourceSet()).ToArray();
        }

        /// <inheritdoc />
        public async Task<bool> Remove(string id, CancellationToken cancellationToken)
        {
            using var session = _sessionFactory();
            session.Delete<OwnedResourceSet>(id);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids)
        {
            using var session = _sessionFactory();
            var resourceSets =
                await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);

            return resourceSets.Select(x => x.AsResourceSet()).ToArray();
        }
    }

    public class OwnedResourceSet : ResourceSet
    {
        /// <summary>
        /// Gets or sets the resource set owner.
        /// </summary>
        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        /// <summary>
        /// Returns the resource set base.
        /// </summary>
        /// <returns></returns>
        public ResourceSet AsResourceSet()
        {
            return new ResourceSet
            {
                AuthorizationPolicies = AuthorizationPolicies,
                Description = Description,
                IconUri = IconUri,
                Id = Id,
                Name = Name,
                Scopes = Scopes,
                Type = Type
            };
        }

        /// <summary>
        /// Create an <see cref="OwnedResourceSet"/> instance from a <see cref="ResourceSet"/> instance.
        /// </summary>
        /// <param name="resourceSet">The base resource set.</param>
        /// <param name="owner">The resource set owner.</param>
        /// <returns></returns>
        public static OwnedResourceSet FromResourceSet(ResourceSet resourceSet, string owner)
        {
            return new OwnedResourceSet
            {
                AuthorizationPolicies = resourceSet.AuthorizationPolicies,
                Description = resourceSet.Description,
                IconUri = resourceSet.IconUri,
                Id = resourceSet.Id,
                Name = resourceSet.Name,
                Owner = owner,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type
            };
        }
    }
}
