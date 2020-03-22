namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the in-memory resource set repository.
    /// </summary>
    /// <seealso cref="IResourceSetRepository" />
    public sealed class InMemoryResourceSetRepository : IResourceSetRepository
    {
        private readonly ICollection<OwnedResourceSet> _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryResourceSetRepository"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        public InMemoryResourceSetRepository(IEnumerable<(string owner, ResourceSet resource)> resources = null)
        {
            _resources = resources?.Select(x => new OwnedResourceSet(x.owner, x.resource)).ToList() ?? new List<OwnedResourceSet>();
        }

        /// <inheritdoc />
        public Task<bool> Remove(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policy = _resources.FirstOrDefault(p => p.Resource.Id == id);
            if (policy == null)
            {
                return Task.FromResult(false);
            }

            _resources.Remove(policy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<ResourceSet> Get(string owner, string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = _resources.FirstOrDefault(p => p.Resource.Id == id);

            return Task.FromResult(rec?.Resource);
        }

        /// <inheritdoc />
        public Task<ResourceSet[]> Get(CancellationToken cancellationToken, params string[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var result = _resources.Where(r => ids.Contains(r.Resource.Id)).Select(x => x.Resource).ToArray();

            return Task.FromResult(result);
        }

        /// <param name="owner"></param>
        /// <param name="cancellationToken"></param>
        /// <inheritdoc />
        public Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken)
        {
            var result = _resources.Where(x => x.Owner == owner).Select(x => x.Resource).ToArray();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> Add(string owner, ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            _resources.Add(new OwnedResourceSet(owner, resourceSet));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<GenericResult<ResourceSet>> Search(
            SearchResourceSet parameter,
            CancellationToken cancellationToken)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = _resources.Select(x => x.Resource);
            if (parameter.Ids != null && parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names != null && parameter.Names.Any())
            {
                result = result.Where(r => parameter.Names.Any(n => r.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                result = result.Where(r => parameter.Types.Any(t => r.Type.Contains(t)));
            }

            var sortedResult = result.OrderBy(c => c.Id).ToArray();
            var nbResult = sortedResult.Length;

            return Task.FromResult(
                new GenericResult<ResourceSet>
                {
                    Content = sortedResult.Skip(parameter.StartIndex).Take(parameter.TotalResults).ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = nbResult
                });
        }

        /// <inheritdoc />
        public Task<bool> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = _resources.FirstOrDefault(p => p.Resource.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.Resource.AuthorizationPolicies = resourceSet.AuthorizationPolicies;
            rec.Resource.IconUri = resourceSet.IconUri;
            rec.Resource.Name = resourceSet.Name;
            rec.Resource.Scopes = resourceSet.Scopes;
            rec.Resource.Type = resourceSet.Type;
            return Task.FromResult(true);
        }

        private class OwnedResourceSet
        {
            public OwnedResourceSet(string owner, ResourceSet resource)
            {
                Owner = owner;
                Resource = resource;
            }

            public string Owner { get; }
            public ResourceSet Resource { get; }
        }
    }
}
