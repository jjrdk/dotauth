namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Properties;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the in-memory resource set repository.
    /// </summary>
    /// <seealso cref="IResourceSetRepository" />
    internal sealed class InMemoryResourceSetRepository : IResourceSetRepository
    {
        private readonly ICollection<OwnedResourceSet> _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryResourceSetRepository"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        public InMemoryResourceSetRepository(IEnumerable<(string owner, ResourceSet resource)>? resources = null)
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
        public Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = _resources.FirstOrDefault(p => p.Owner == owner && p.Resource.Id == id);

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

        /// <inheritdoc />
        public Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
        {
            var owners = _resources.Where(r => ids.Contains(r.Resource.Id)).Select(x => x.Owner).Distinct();

            return Task.FromResult(owners.SingleOrDefault());
        }

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
        public Task<PagedResult<ResourceSet>> Search(
            SearchResourceSet parameter,
            CancellationToken cancellationToken)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = _resources.Select(x => x.Resource);
            if (parameter.Ids.Any())
            {
                result = result.Where(r => parameter.Ids.Contains(r.Id));
            }

            if (parameter.Names.Any())
            {
                result = result.Where(
                    r => parameter.Names.Any(n => !string.IsNullOrWhiteSpace(r.Name) && r.Name.Contains(n)));
            }

            if (parameter.Types.Any())
            {
                result = result.Where(
                    r => parameter.Types.Any(t => !string.IsNullOrWhiteSpace(r.Type) && r.Type.Contains(t)));
            }

            var sortedResult = result.OrderBy(c => c.Id).ToArray();
            var nbResult = sortedResult.Length;
            if (parameter.TotalResults > 0)
            {
                sortedResult = sortedResult.Skip(parameter.StartIndex).Take(parameter.TotalResults).ToArray();
            }
            return Task.FromResult(
                new PagedResult<ResourceSet>
                {
                    Content = sortedResult,
                    StartIndex = parameter.StartIndex,
                    TotalResults = nbResult
                });
        }

        /// <inheritdoc />
        public Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = _resources.FirstOrDefault(p => p.Resource.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult<Option>(new Option.Error(new ErrorDetails
                {
                    Status = HttpStatusCode.NotFound,
                    Title = ErrorCodes.NotUpdated,
                    Detail = SharedStrings.ResourceCannotBeUpdated
                }));
            }

            _resources.Remove(rec);
            var res = rec.Resource with
            {
                AuthorizationPolicies = resourceSet.AuthorizationPolicies,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type
            };
            rec = new OwnedResourceSet(rec.Owner, res);
            _resources.Add(rec);
            return Task.FromResult<Option>(new Option.Success());
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
