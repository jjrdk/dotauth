namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using SimpleAuth.Shared.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class InMemoryResourceSetRepository : IResourceSetRepository
    {
        private readonly ICollection<ResourceSet> _resources;

        public InMemoryResourceSetRepository(IReadOnlyCollection<ResourceSet> resources = null)
        {
            _resources = resources?.ToList() ?? new List<ResourceSet>();
        }

        /// <inheritdoc />
        public Task<bool> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var policy = _resources.FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return Task.FromResult(false);
            }

            _resources.Remove(policy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<ResourceSet> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == id);
            if (rec == null)
            {
                return Task.FromResult((ResourceSet) null);
            }

            return Task.FromResult(rec);
        }

        /// <inheritdoc />
        public Task<ResourceSet[]> Get(params string[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var result = _resources.Where(r => ids.Contains(r.Id)).ToArray();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<ResourceSet[]> GetAll()
        {
            var result = _resources.ToArray();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> Insert(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            _resources.Add(resourceSet);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<GenericResult<ResourceSet>> Search(SearchResourceSet parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ResourceSet> result = _resources;
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

            var nbResult = result.Count();
            result = result.OrderBy(c => c.Id);
            if (parameter.TotalResults > 0)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.TotalResults);
            }

            return Task.FromResult(
                new GenericResult<ResourceSet>
                {
                    Content = result.ToArray(), StartIndex = parameter.StartIndex, TotalResults = nbResult
                });
        }

        /// <inheritdoc />
        public Task<bool> Update(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            var rec = _resources.FirstOrDefault(p => p.Id == resourceSet.Id);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.AuthorizationPolicyIds = resourceSet.AuthorizationPolicyIds;
            rec.IconUri = resourceSet.IconUri;
            rec.Name = resourceSet.Name;
            rec.Policies = resourceSet.Policies;
            rec.Scopes = resourceSet.Scopes;
            rec.Type = resourceSet.Type;
            rec.Uri = resourceSet.Uri;
            return Task.FromResult(true);
        }
    }
}
